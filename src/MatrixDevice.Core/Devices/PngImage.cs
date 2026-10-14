using SkiaSharp;

namespace MatrixDevice.Core;

public static class PngImage
{
    private static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];
    public static (int Width, int Height) ReadDimensions(ReadOnlySpan<byte> png)
    {
        using var image = Decode(png);
        return (image.Width, image.Height);
    }
    public static byte[] Create(int width, int height, Func<int, int, (byte R, byte G, byte B)> pixel)
    {
        ValidateSize(width, height);
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        for (var y = 0; y < height; y++) for (var x = 0; x < width; x++)
        { var color = pixel(x, y); bitmap.SetPixel(x, y, new SKColor(color.R, color.G, color.B)); }
        return Encode(bitmap);
    }
    public static byte[] Crop(ReadOnlySpan<byte> png, int x, int y, int width, int height)
    {
        using var source = Decode(png);
        if (x < 0 || y < 0 || width <= 0 || height <= 0 || (long)x + width > source.Width || (long)y + height > source.Height)
            throw new InvalidDataException($"Crop ({x},{y},{width},{height}) is outside PNG {source.Width}x{source.Height}.");
        ValidateSize(width, height);
        using var cropped = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var sourceImage = SKImage.FromBitmap(source);
        using (var canvas = new SKCanvas(cropped)) canvas.DrawImage(sourceImage, new SKRect(x, y, x + width, y + height), new SKRect(0, 0, width, height), new SKSamplingOptions(SKFilterMode.Nearest));
        return Encode(cropped);
    }
    public static (byte R, byte G, byte B) ColorAt(ReadOnlySpan<byte> png, int x, int y)
    {
        using var image = Decode(png);
        if (x < 0 || x >= image.Width || y < 0 || y >= image.Height) throw new ArgumentOutOfRangeException(nameof(x));
        var color = image.GetPixel(x, y); return (color.Red, color.Green, color.Blue);
    }
    private static SKBitmap Decode(ReadOnlySpan<byte> png)
    {
        if (png.Length < 33 || !png[..8].SequenceEqual(Signature)) throw new InvalidDataException("Screenshot is not a PNG image.");
        using var data = SKData.CreateCopy(png.ToArray());
        using var codec = SKCodec.Create(data) ?? throw new InvalidDataException("PNG decoder could not open screenshot.");
        ValidateSize(codec.Info.Width, codec.Info.Height);
        var bitmap = new SKBitmap(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        var result = codec.GetPixels(bitmap.Info, bitmap.GetPixels());
        if (result != SKCodecResult.Success) { bitmap.Dispose(); throw new InvalidDataException($"PNG decoding failed: {result}."); }
        return bitmap;
    }
    private static byte[] Encode(SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var bytes = image.Encode(SKEncodedImageFormat.Png, 100) ?? throw new InvalidDataException("PNG encoding failed.");
        return bytes.ToArray();
    }
    private static void ValidateSize(int width, int height)
    { if (width <= 0 || height <= 0 || (long)width * height > 24_000_000) throw new InvalidDataException($"Unsupported PNG size: {width}x{height}."); }
}
