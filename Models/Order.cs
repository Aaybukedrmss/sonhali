using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_store.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    [MaxLength(32)]
    public string OrderNumber { get; set; } = null!; // unique

    [Required]
    public int UserId { get; set; }
    public AppUser? User { get; set; }

    [MaxLength(20)]
    public string ShippingProvider { get; set; } = null!;

    public int? AddressId { get; set; }
    public UserAddress? Address { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ShippingCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Paid, Cancelled, Shipped

    // Iyzipay tracking
    [MaxLength(64)]
    public string? PaymentId { get; set; }

    [MaxLength(64)]
    public string? ConversationId { get; set; }

    [MaxLength(32)]
    public string? PaymentStatus { get; set; }

    [MaxLength(256)]
    public string? PaymentLastError { get; set; }
}


