using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_store.Models;

public class CartItem
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public AppUser? User { get; set; }

    [Required]
    public int UrunId { get; set; }
    public Urun? Urun { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


