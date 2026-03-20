using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using techviet_be.Common.Options;

namespace techviet_be.Service;

public class ImageService(IOptions<CloudinaryOptions> cloudinaryOptions) : IImageService
{
    private readonly Cloudinary _cloudinary = new(new Account(
        cloudinaryOptions.Value.CloudName,
        cloudinaryOptions.Value.ApiKey,
        cloudinaryOptions.Value.ApiSecret));

    public async Task<string> UploadAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
        {
            throw new InvalidOperationException("File is empty.");
        }

        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "techviet/products"
        };

        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
        if (result.Error is not null)
        {
            throw new InvalidOperationException(result.Error.Message);
        }

        return result.SecureUrl.ToString();
    }
}