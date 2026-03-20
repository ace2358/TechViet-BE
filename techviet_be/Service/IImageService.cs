namespace techviet_be.Service;

public interface IImageService
{
    Task<string> UploadAsync(IFormFile file, CancellationToken cancellationToken = default);
}
