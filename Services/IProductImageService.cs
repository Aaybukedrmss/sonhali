using dotnet_store.Models;

namespace dotnet_store.Services;

public interface IProductImageService
{
    Task<string?> GetPrimaryImageUrlByIsbnAsync(long isbn, CancellationToken cancellationToken = default);
}


