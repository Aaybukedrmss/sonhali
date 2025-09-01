using System.ComponentModel.DataAnnotations;

namespace dotnet_store.Models;

public class Kategori
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Kategori adı zorunludur")]
    [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir")]
    public string KategoriAdi { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
    public string Aciklama { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "URL en fazla 100 karakter olabilir")]
    public string Url { get; set; } = string.Empty;
    
    public bool Aktif { get; set; }
}
