namespace dotnet_store.Models;

public class SliderCreateModel
{
    public string? Baslik { get; set; }
    public string? Aciklama { get; set; }
    public IFormFile? Resim { get; set; } = null!;
    public int Index { get; set; }//111111
    public bool Aktif { get; set; }
}

