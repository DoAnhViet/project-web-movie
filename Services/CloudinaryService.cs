using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using WebMovie.Services;
public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> config)
    {
        var settings = config.Value;

        if (string.IsNullOrEmpty(settings.CloudName))
            throw new Exception("Cloudinary CloudName missing in configuration!");

        var acc = new Account(
            settings.CloudName,
            settings.ApiKey,
            settings.ApiSecret
        );

        _cloudinary = new Cloudinary(acc);
        _cloudinary.Api.Secure = true; // bắt buộc để link avatar dạng HTTPS
    }

    public async Task<string?> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, file.OpenReadStream()),
            Folder = "webmovie/avatars"
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        return result?.SecureUrl?.ToString();
    }
}
