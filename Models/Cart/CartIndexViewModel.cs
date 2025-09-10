using System.ComponentModel.DataAnnotations;

namespace dotnet_store.Models;

public class CartIndexViewModel
{
    public List<CartItem> Items { get; set; } = new();

    [DataType(DataType.Currency)]
    public decimal Subtotal { get; set; }

    [DataType(DataType.Currency)]
    public decimal Shipping { get; set; } = 0m;

    [DataType(DataType.Currency)]
    public decimal Total => Subtotal + Shipping;
}


