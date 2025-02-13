using SkiaSharp;

namespace Citlali.Services;

public class UtilitiesService(Models.Configuration configuration)
{
    private readonly Models.Configuration _configuration = configuration;

    public byte[] ProcessProfileImage(IFormFile image)
    {
        using var stream = image.OpenReadStream();
        SKBitmap skBitmap = SKBitmap.Decode(stream);

        int croppedSize = Math.Min(skBitmap.Width, skBitmap.Height);
        int x = (skBitmap.Width - croppedSize) / 2;
        int y = (skBitmap.Height - croppedSize) / 2;

        SKBitmap croppedImage = new(croppedSize, croppedSize);
        using (SKCanvas canvas = new(croppedImage))
        {
            canvas.DrawBitmap(skBitmap, new SKRect(x, y, x + croppedSize, y + croppedSize), new SKRect(0, 0, croppedSize, croppedSize));
        }

        var maxSize = _configuration.User.ProfileImageMaxSize;
        SKBitmap resizedImage = croppedImage;
        if (croppedSize > maxSize)
        {
            resizedImage = croppedImage.Resize(new SKImageInfo(maxSize, maxSize), SKSamplingOptions.Default);
            croppedImage.Dispose();
        }

        var encodeQuality = _configuration.User.ProfileImageEncodeQuality;
        var imageFormat = _configuration.User.ProfileImageFormat;
        SKData encoded = resizedImage.Encode(imageFormat, encodeQuality);
        byte[] bytes = encoded.ToArray();
        encoded.Dispose();

        return bytes;
    }
}
