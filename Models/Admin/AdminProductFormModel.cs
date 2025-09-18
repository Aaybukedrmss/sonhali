using System.ComponentModel.DataAnnotations;

namespace dotnet_store.Models.Admin;

public class AdminProductFormModel
{
    [Required(ErrorMessage = "Ürün adı zorunludur")]
    [StringLength(200)]
    public string ProductName { get; set; } = null!;

    [Required(ErrorMessage = "SKU zorunludur")]
    public int Sku { get; set; }

    [Required(ErrorMessage = "ISBN zorunludur")]
    public long Isbn { get; set; }

    [Range(0, short.MaxValue)]
    public short LeadTimeHour { get; set; }

    [Range(0, double.MaxValue)]
    public double Fiyat { get; set; }

    public bool Aktif { get; set; }

    [StringLength(1000)]
    public string Degerlendirmelerim { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [Required]
    public string FullDescription { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Yayinevi { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Marka { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string MedyaTipi { get; set; } = string.Empty;

    [Range(1, short.MaxValue)]
    public short TopMostCategoryId { get; set; }

    [Required]
    public string TopMostCategoryNames { get; set; } = string.Empty;

    [Range(1, short.MaxValue)]
    public short CategoryId { get; set; }

    [Required]
    [StringLength(100)]
    public string CategorySlug { get; set; } = string.Empty;

    [Range(0, 5)]
    public short TotalRating { get; set; }

    public IFormFile? Resim { get; set; }
}


