using System.Text.Json;
using Microsoft.Data.Sqlite;
using MatrixDevice.Core;

namespace MatrixDevice.Host;

public sealed record DeviceMetadata(string Serial, string Name = "", string GroupId = "", string Remark = "");
public sealed record GroupRecord(string Id, string Name);

public sealed class CatalogRepository : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly object _gate = new();
    private static readonly HashSet<string> Categories = ["Group", "DeviceGroupRelation", "DeviceRemarkRelation", "EthernetScan", "CustomDeviceCommand", "ScriptDeviceConfiguration", "EnvironmentConfig", "Proxy", "Server", "AppCk", "User"];
    public CatalogRepository(string path)
    {
        path = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        _connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = path, Pooling = false }.ToString());
        try
        {
        _connection.Open();
        using var integrity = _connection.CreateCommand();
        integrity.CommandText = "PRAGMA integrity_check;";
        if (!string.Equals(integrity.ExecuteScalar()?.ToString(), "ok", StringComparison.OrdinalIgnoreCase))
            throw new IOException("SQLite integrity check failed: " + path);
        using var version = _connection.CreateCommand();
        version.CommandText = "PRAGMA user_version;";
        var previous = Convert.ToInt32(version.ExecuteScalar());
        if (previous > 1) throw new IOException("Unsupported database schema version: " + previous);
        if (previous == 0)
        {
            if (new FileInfo(path).Length > 0)
            {
                using var backup = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = path + ".pre-migration.bak", Pooling = false }.ToString());
                backup.Open();
                _connection.BackupDatabase(backup);
            }
            using var transaction = _connection.BeginTransaction();
            foreach (var category in Categories)
            {
                using var create = _connection.CreateCommand();
                create.Transaction = transaction;
                create.CommandText = $"CREATE TABLE IF NOT EXISTS [{category}] (Id TEXT PRIMARY KEY NOT NULL, Payload TEXT NOT NULL, UpdatedUtc TEXT NOT NULL);";
                create.ExecuteNonQuery();
            }
            using var migration = _connection.CreateCommand();
            migration.Transaction = transaction;
            migration.CommandText = "CREATE TABLE IF NOT EXISTS DeviceMetadata(Serial TEXT PRIMARY KEY, Name TEXT NOT NULL, GroupId TEXT NOT NULL, Remark TEXT NOT NULL); PRAGMA user_version=1;";
            migration.ExecuteNonQuery();
            transaction.Commit();
        }
        }
        catch { _connection.Dispose(); throw; }
    }

    public DeviceMetadata GetMetadata(string serial)
    {
        lock (_gate)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT Serial, Name, GroupId, Remark FROM DeviceMetadata WHERE Serial=$serial;";
            command.Parameters.AddWithValue("$serial", serial);
            using var reader = command.ExecuteReader();
            return reader.Read() ? new(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)) : new(serial);
        }
    }
    public IReadOnlyList<DeviceMetadata> GetAllMetadata()
    {
        lock (_gate)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT Serial, Name, GroupId, Remark FROM DeviceMetadata ORDER BY Serial;";
            using var reader = command.ExecuteReader();
            List<DeviceMetadata> rows = [];
            while (reader.Read()) rows.Add(new(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
            return rows;
        }
    }
    public void UpsertMetadata(string serial, string name, string groupId)
    {
        if (string.IsNullOrWhiteSpace(serial)) throw new ArgumentException("Serial is required", nameof(serial));
        lock (_gate)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "INSERT INTO DeviceMetadata VALUES($serial,$name,$group,'') ON CONFLICT(Serial) DO UPDATE SET Name=excluded.Name,GroupId=excluded.GroupId;";
            command.Parameters.AddWithValue("$serial", serial);
            command.Parameters.AddWithValue("$name", name);
            command.Parameters.AddWithValue("$group", groupId);
            command.ExecuteNonQuery();
        }
    }
    public void SetRemark(string serial, string remark)
    {
        lock (_gate)
        {
            var old = GetMetadata(serial);
            UpsertMetadata(serial, old.Name, old.GroupId);
            using var command = _connection.CreateCommand();
            command.CommandText = "UPDATE DeviceMetadata SET Remark=$remark WHERE Serial=$serial;";
            command.Parameters.AddWithValue("$remark", remark);
            command.Parameters.AddWithValue("$serial", serial);
            command.ExecuteNonQuery();
        }
    }
    public void Put<T>(string category, string id, T value)
    {
        ValidateCategory(category);
        lock (_gate)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = $"INSERT INTO [{category}] VALUES($id,$payload,$at) ON CONFLICT(Id) DO UPDATE SET Payload=excluded.Payload,UpdatedUtc=excluded.UpdatedUtc;";
            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$payload", JsonSerializer.Serialize(value, JsonDefaults.Options));
            command.Parameters.AddWithValue("$at", DateTimeOffset.UtcNow.ToString("O"));
            command.ExecuteNonQuery();
        }
    }
    public IReadOnlyList<T> GetAll<T>(string category)
    {
        ValidateCategory(category);
        lock (_gate)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = $"SELECT Payload FROM [{category}] ORDER BY Id;";
            using var reader = command.ExecuteReader();
            List<T> rows = [];
            while (reader.Read()) rows.Add(JsonSerializer.Deserialize<T>(reader.GetString(0), JsonDefaults.Options)!);
            return rows;
        }
    }
    public void Delete(string category, string id)
    {
        ValidateCategory(category);
        lock (_gate)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = $"DELETE FROM [{category}] WHERE Id=$id;";
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }
    }
    private static void ValidateCategory(string category)
    {
        if (!Categories.Contains(category)) throw new ArgumentException("Unknown entity category: " + category);
    }
    public void Dispose() => _connection.Dispose();
}
