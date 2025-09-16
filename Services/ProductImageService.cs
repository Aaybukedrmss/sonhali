using dotnet_store.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_store.Services;

public class ProductImageService : IProductImageService
{
    private readonly DataContext _context;

    public ProductImageService(DataContext context)
    {
        _context = context;
    }

    public async Task<string?> GetPrimaryImageUrlByIsbnAsync(long isbn, CancellationToken cancellationToken = default)
    {
        // resimurun tablosunda tek kayıt var denildi. Url alanını tercih ediyoruz.
        var record = await _context.ResimUrunler
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Isbn == isbn, cancellationToken);

        if (record == null)
        {
            return null;
        }

        var url = record.Url?.Trim();
        var imageName = record.Images?.Trim();

        // Öncelik: images sütunu
        if (!string.IsNullOrWhiteSpace(imageName))
        {
            // Eğer zaten http(s) ya da kök ile başlıyorsa olduğu gibi döndür
            if (imageName.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                imageName.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                imageName.StartsWith("/"))
            {
                return imageName;
            }

            return "/img/" + imageName;
        }

        // Fallback: Url sütunu
        if (!string.IsNullOrWhiteSpace(url))
        {
            return url;
        }

        return null;
    }
}


