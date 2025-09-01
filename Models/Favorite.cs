namespace dotnet_store.Models;

public class Favorite
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public int UrunId { get; set; }
    public Urun Urun { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


