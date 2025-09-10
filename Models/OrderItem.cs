using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_store.Models;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int UrunId { get; set; }
    public Urun? Urun { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [NotMapped]
    public decimal LineTotal => UnitPrice * Quantity;
}


