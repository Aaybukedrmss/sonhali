using System.ComponentModel.DataAnnotations;

namespace dotnet_store.Models;

public class ResetPasswordModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni Şifre")]
    public string Password { get; set; } = null!;

    [DataType(DataType.Password)]
    [Display(Name = "Yeni Şifre (Tekrar)")]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
    public string ConfirmPassword { get; set; } = null!;

    public string Token { get; set; } = null!;
}


