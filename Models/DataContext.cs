using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace dotnet_store.Models;

public class DataContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<Urun> Urunler { get; set; }
    public DbSet<Kategori> Kategoriler { get; set; }
    public DbSet<Slider> Sliders { get; set; }
    public DbSet<LoginLog> LoginLogs { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<SupportRequest> SupportRequests { get; set; }
    public DbSet<UserAddress> UserAddresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Tablo adını ShopCo olarak belirt
        modelBuilder.Entity<Urun>().ToTable("ShopCo");

        // Favoriler için benzersiz kullanıcı-ürün çifti
        modelBuilder.Entity<Favorite>()
            .HasIndex(f => new { f.UserId, f.UrunId })
            .IsUnique();

        // Her kullanıcı için en fazla 1 varsayılan adres
        modelBuilder.Entity<UserAddress>()
            .HasIndex(a => new { a.UserId, a.IsDefault })
            .IsUnique()
            .HasFilter("[IsDefault] = 1");
    }
}
