using System.ComponentModel.DataAnnotations;

namespace dotnet_store.Models;

public class Slider
{
    public int Id { get; set; }
    
    [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
    public string? Baslik { get; set; }
    
    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
    public string? Aciklama { get; set; }
    
    [Required(ErrorMessage = "Resim zorunludur")]
    [StringLength(255, ErrorMessage = "Resim yolu en fazla 255 karakter olabilir")]
    public string Resim { get; set; } = null!;
    
    [Range(0, int.MaxValue, ErrorMessage = "Index 0'dan büyük olmalıdır")]
    public int Index { get; set; }
    
    public bool Aktif { get; set; }
}
