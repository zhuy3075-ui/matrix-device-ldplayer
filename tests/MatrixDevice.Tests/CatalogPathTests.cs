using System.Text.Json;
using MatrixDevice.Host;
using Microsoft.Data.Sqlite;

public static class CatalogPathTests
{
    private const string FileName = "catalog-\u4e2d\u6587 #%;.sqlite3";
    private const string BackupSuffix = ".pre-migration.bak";

    public static void Register(Action<string, Func<Task>> test)
    {
        test("SQLite ordinary short literal path preserves CRUD through fresh reopen", ShortPathAsync);
        if (!OperatingSystem.IsWindows())
        {
            Console.WriteLine("SKIP Windows SQLite catalog path cases: Windows local paths are required.");
            return;
        }
        foreach (var length in new[] { 251, 252, 253, 300 })
        {
            var exactLength = length;
            test($"SQLite Windows ordinary {exactLength}-character literal path preserves CRUD through fresh reopen", () => OrdinaryPathAsync(exactLength));
        }
        test("SQLite Windows extended local path reopens the same ordinary catalog", ExtendedPathAsync);
        test("SQLite Windows ordinary long legacy catalog preserves records and original migration backup", LegacyMigrationAsync);
        test("SQLite Windows ordinary long rejected catalogs preserve files and release handles", RejectedCatalogsAsync);
    }

    private static Task ShortPathAsync()
    {
        using var directory = new CatalogPathDirectory();
        ExerciseCrud(directory.ShortPath);
        return Task.CompletedTask;
    }

    private static Task OrdinaryPathAsync(int length)
    {
        using var directory = new CatalogPathDirectory();
        var path = directory.AtLength(length);
        AssertOrdinaryWindowsPath(path, length);
        ExerciseCrud(path);
        return Task.CompletedTask;
    }

    private static void ExerciseCrud(string path)
    {
        Assert(Path.IsPathFullyQualified(path), "Catalog fixture path is not absolute: " + path);
        Assert(!path.StartsWith(@"\\?\", StringComparison.Ordinal), "Ordinary catalog input was already extended: " + path);
        const string serial = "catalog-'#%;";
        using (var repository = new CatalogRepository(path))
        {
            Assert(repository.GetAllMetadata().Count == 0 && repository.GetAll<JsonElement>("User").Count == 0, "Fresh catalog already contained rows.");
            repository.UpsertMetadata(serial, "Original", "group-1");
            repository.SetRemark(serial, "remark-\u4e2d\u6587 #%;");
            repository.UpsertMetadata(serial, "Updated", "group-2");
            AssertMetadata(repository, serial, "Updated", "group-2", "remark-\u4e2d\u6587 #%;");
            repository.Put("User", "keep-'#%;", new { value = 1 });
            repository.Put("User", "keep-'#%;", new { value = 2 });
            repository.Put("User", "remove", new { value = 9 });
            Assert(repository.GetAll<JsonElement>("User").Count == 2, "Catalog entity update inserted a duplicate row.");
            repository.Delete("User", "remove");
            Assert(repository.GetAll<JsonElement>("User").Single().GetProperty("value").GetInt32() == 2, "Catalog entity update or delete was lost.");
        }
        Assert(File.Exists(path), "Catalog was not created at its literal requested path: " + path);
        Assert(!File.Exists(path + BackupSuffix), "A fresh empty catalog created a migration backup.");
        using (var reopened = new CatalogRepository(path))
        {
            AssertMetadata(reopened, serial, "Updated", "group-2", "remark-\u4e2d\u6587 #%;");
            Assert(reopened.GetAllMetadata().Count == 1 && reopened.GetAll<JsonElement>("User").Single().GetProperty("value").GetInt32() == 2, "Catalog rows did not survive a fresh connection.");
            reopened.Put("User", "keep-'#%;", new { value = 3 });
            reopened.Delete("User", "missing");
        }
        using (var reopened = new CatalogRepository(path))
            Assert(reopened.GetAll<JsonElement>("User").Single().GetProperty("value").GetInt32() == 3, "Reopened catalog could not persist a later write.");
        Assert(!File.Exists(path + BackupSuffix), "Reopening schema version 1 created a migration backup.");
    }

    private static Task ExtendedPathAsync()
    {
        using var directory = new CatalogPathDirectory();
        var path = directory.AtLength(300);
        AssertOrdinaryWindowsPath(path, 300);
        var extended = ExtendedFixturePath(path);
        using (var repository = new CatalogRepository(extended))
            repository.UpsertMetadata("extended", "First", "group");
        Assert(File.Exists(path), "Extended path created a different catalog from its ordinary local path.");
        using (var repository = new CatalogRepository(path))
        {
            AssertMetadata(repository, "extended", "First", "group", "");
            repository.SetRemark("extended", "ordinary reopen");
        }
        using (var repository = new CatalogRepository(extended))
            AssertMetadata(repository, "extended", "First", "group", "ordinary reopen");
        Assert(!File.Exists(path + BackupSuffix), "Extended path schema version 1 reopen created a backup.");
        return Task.CompletedTask;
    }

    private static Task LegacyMigrationAsync()
    {
        using var directory = new CatalogPathDirectory();
        var path = directory.AtLength(300);
        AssertOrdinaryWindowsPath(path, 300);
        using (var original = OpenFixture(path))
            Execute(original, "CREATE TABLE DeviceMetadata(Serial TEXT PRIMARY KEY, Name TEXT NOT NULL, GroupId TEXT NOT NULL, Remark TEXT NOT NULL); CREATE TABLE [User](Id TEXT PRIMARY KEY NOT NULL, Payload TEXT NOT NULL, UpdatedUtc TEXT NOT NULL); INSERT INTO DeviceMetadata VALUES('legacy','Original','old-group','old-remark'); INSERT INTO [User] VALUES('original','{\"value\":7}','2000-01-01T00:00:00Z'); PRAGMA user_version=0;");
        using (var migrated = new CatalogRepository(path))
        {
            AssertMetadata(migrated, "legacy", "Original", "old-group", "old-remark");
            Assert(migrated.GetAll<JsonElement>("User").Single().GetProperty("value").GetInt32() == 7, "Migration lost the original entity payload.");
            migrated.SetRemark("legacy", "after migration");
            migrated.Put("User", "original", new { value = 8 });
        }
        var backupPath = path + BackupSuffix;
        Assert(File.Exists(backupPath), "Migration backup did not use the original catalog directory and suffix.");
        var backupBytes = File.ReadAllBytes(backupPath);
        using (var backup = OpenFixture(backupPath))
        {
            Assert(ScalarInt(backup, "PRAGMA user_version;") == 0, "Migration backup already contains the new schema version.");
            Assert(ScalarText(backup, "SELECT Name || '|' || GroupId || '|' || Remark FROM DeviceMetadata WHERE Serial='legacy';") == "Original|old-group|old-remark", "Migration backup lost the original metadata or contains later changes.");
            Assert(ScalarText(backup, "SELECT Payload FROM [User] WHERE Id='original';") == "{\"value\":7}", "Migration backup lost the original entity payload.");
            Assert(ScalarText(backup, "PRAGMA integrity_check;") == "ok", "Migration backup cannot be read as an intact SQLite database.");
        }
        using (var reopened = new CatalogRepository(path))
        {
            AssertMetadata(reopened, "legacy", "Original", "old-group", "after migration");
            Assert(reopened.GetAll<JsonElement>("User").Single().GetProperty("value").GetInt32() == 8, "Migrated catalog did not persist a later entity write.");
        }
        using (var current = OpenFixture(path))
            Assert(ScalarInt(current, "PRAGMA user_version;") == 1, "Catalog migration did not persist schema version 1.");
        Assert(File.ReadAllBytes(backupPath).SequenceEqual(backupBytes), "Schema version 1 reopen rewrote the original migration backup.");
        return Task.CompletedTask;
    }

    private static Task RejectedCatalogsAsync()
    {
        using var directory = new CatalogPathDirectory();
        var path = directory.AtLength(300);
        AssertOrdinaryWindowsPath(path, 300);
        using (var future = OpenFixture(path))
            Execute(future, "CREATE TABLE FutureMarker(Value TEXT NOT NULL); INSERT INTO FutureMarker VALUES('preserve'); PRAGMA user_version=2;");
        var futureBytes = File.ReadAllBytes(path);
        for (var attempt = 0; attempt < 2; attempt++)
        {
            var failure = Expect<IOException>(() => { using var rejected = new CatalogRepository(path); });
            Assert(failure.Message.Contains("Unsupported database schema version: 2", StringComparison.Ordinal), "Future schema failed for an unrelated reason: " + failure.Message);
            AssertExclusiveOpen(path);
            Assert(File.ReadAllBytes(path).SequenceEqual(futureBytes), "Future schema rejection changed the source database.");
        }
        Assert(!File.Exists(path + BackupSuffix), "Future schema rejection created a migration backup.");
        using (var future = OpenFixture(path))
        {
            Assert(ScalarInt(future, "PRAGMA user_version;") == 2, "Future schema version changed after rejection.");
            Assert(ScalarText(future, "SELECT Value FROM FutureMarker;") == "preserve", "Future schema contents changed after rejection.");
        }
        var corruptBytes = new byte[] { 84, 104, 105, 115, 32, 105, 115, 32, 110, 111, 116, 32, 83, 81, 76, 105, 116, 101 };
        File.WriteAllBytes(path, corruptBytes);
        Expect<SqliteException>(() => { using var rejected = new CatalogRepository(path); });
        AssertExclusiveOpen(path);
        Assert(File.ReadAllBytes(path).SequenceEqual(corruptBytes) && !File.Exists(path + BackupSuffix), "Corrupt catalog rejection changed the original bytes or created a backup.");
        return Task.CompletedTask;
    }

    private static void AssertMetadata(CatalogRepository repository, string serial, string name, string group, string remark)
    {
        var row = repository.GetMetadata(serial);
        Assert(row == new DeviceMetadata(serial, name, group, remark), "Catalog metadata differs from the expected record: " + row);
    }

    private static void AssertOrdinaryWindowsPath(string path, int length)
    {
        Assert(OperatingSystem.IsWindows(), "Windows path semantics require Windows.");
        Assert(Path.IsPathFullyQualified(path) && path.Length >= 3 && path[1] == ':', "Windows catalog fixture requires an absolute local drive path: " + path);
        Assert(!path.StartsWith(@"\\?\", StringComparison.Ordinal) && path.Length == length, $"Expected ordinary path length {length}; got {path.Length}: {path}");
    }

    private static string ExtendedFixturePath(string path)
    {
        Assert(OperatingSystem.IsWindows() && path.Length >= 3 && path[1] == ':' && !path.StartsWith(@"\\", StringComparison.Ordinal), "Native fixture path must be an ordinary Windows local drive path: " + path);
        return @"\\?\" + path;
    }

    // Raw fixture connections prepare old schemas independently of repository path handling.
    private static SqliteConnection OpenFixture(string path)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = ExtendedFixturePath(path), Pooling = false }.ToString());
        try { connection.Open(); return connection; }
        catch { connection.Dispose(); throw; }
    }

    private static void Execute(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand(); command.CommandText = sql; command.ExecuteNonQuery();
    }

    private static int ScalarInt(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand(); command.CommandText = sql; return Convert.ToInt32(command.ExecuteScalar());
    }

    private static string? ScalarText(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand(); command.CommandText = sql; return command.ExecuteScalar()?.ToString();
    }

    private static T Expect<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T error) { return error; }
        throw new Exception("Expected " + typeof(T).Name);
    }

    private static void AssertExclusiveOpen(string path)
    {
        using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private sealed class CatalogPathDirectory : IDisposable
    {
        private const string Prefix = "catalog-path-";
        private readonly string _parent = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "MatrixDevice.Tests"));
        private readonly string _name = Prefix + Guid.NewGuid().ToString("N");
        private readonly string _root;

        public CatalogPathDirectory()
        {
            _root = Path.GetFullPath(Path.Combine(_parent, _name));
            Directory.CreateDirectory(_root);
        }

        public string ShortPath => Path.Combine(_root, FileName);

        public string AtLength(int length)
        {
            var directory = _root;
            var remaining = length - ShortPath.Length;
            Assert(remaining >= 2, $"Temporary root is too long to construct catalog path length {length}: {_root}");
            while (remaining > 0)
            {
                var componentLength = Math.Min(80, remaining - 1);
                if (remaining - componentLength - 1 == 1) componentLength--;
                Assert(componentLength > 0, "Cannot construct a nonempty path component for length " + length);
                directory = Path.Combine(directory, new string('p', componentLength));
                remaining -= componentLength + 1;
            }
            var path = Path.Combine(directory, FileName);
            Assert(path.Length == length && Path.GetRelativePath(_root, path).Split(Path.DirectorySeparatorChar).All(part => part.Length < 255), "Catalog fixture path length or component bound is incorrect: " + path);
            Directory.CreateDirectory(directory);
            return path;
        }

        public void Dispose()
        {
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var root = Path.GetFullPath(_root);
            Assert(string.Equals(Path.GetDirectoryName(root), _parent, comparison) && string.Equals(Path.GetFileName(root), _name, StringComparison.Ordinal) && _name.StartsWith(Prefix, StringComparison.Ordinal) && Guid.TryParseExact(_name[Prefix.Length..], "N", out _), "Refusing cleanup outside the owned fresh catalog directory: " + root);
            foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
                AssertExclusiveOpen(file);
            Directory.Delete(root, true);
            Assert(!Directory.Exists(root), "Owned catalog fixture directory survived cleanup: " + root);
        }
    }
}
