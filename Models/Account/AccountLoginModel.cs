using System.ComponentModel.DataAnnotations;

namespace dotnet_store.Models;

public class AccountLoginModel
{
    [Required]
    [Display(Name = "Kullanıcı adı veya E-posta")]
    public string UsernameOrEmail { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = null!;

    [Display(Name = "Beni hatırla")]
    public bool RememberMe { get; set; }
}


