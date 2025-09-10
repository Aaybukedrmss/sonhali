using System.ComponentModel.DataAnnotations;

namespace dotnet_store.Models;

public class CheckoutViewModel
{
    public List<CartItem> Items { get; set; } = new();

    [DataType(DataType.Currency)]
    public decimal Subtotal { get; set; }

    [Display(Name = "Kargo Şirketi")]
    [Required(ErrorMessage = "Kargo şirketi seçiniz")] 
    public string ShippingProvider { get; set; } = "Yurtici"; // e.g. Yurtici, Aras, MNG

    [Display(Name = "Adres")]
    [Required(ErrorMessage = "Adres seçiniz")]
    public int SelectedAddressId { get; set; }

    public List<UserAddress> Addresses { get; set; } = new();

    public decimal ShippingCost { get; set; }

    public decimal Total => Subtotal + ShippingCost;

    public List<ShippingOption> ShippingOptions { get; set; } = new();
}

public class ShippingOption
{
    public string Provider { get; set; } = string.Empty; // "Yurtici", "Aras", "Mng"
    public string DisplayName { get; set; } = string.Empty; // Görünen ad
    public decimal Cost { get; set; }
}


