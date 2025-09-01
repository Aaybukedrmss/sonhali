using System.ComponentModel.DataAnnotations;

namespace dotnet_store.Models;

public class ProfileUpdateModel
{
    [Required]
    [Display(Name = "Adınız")]
    public string FirstName { get; set; } = null!;

    [Required]
    [Display(Name = "Soyadınız")]
    public string LastName { get; set; } = null!;

    [Required]
    [Display(Name = "Kullanıcı Adı")]
    [RegularExpression("[a-z0-9@._-]+", ErrorMessage = "Geçersiz kullanıcı adı karakterleri.")]
    public string Username { get; set; } = null!;

    [Required]
    [EmailAddress]
    [Display(Name = "E-Posta Adresi")]
    public string Email { get; set; } = null!;

    [Phone]
    [Display(Name = "Telefon")]
    public string? Phone { get; set; }
}


