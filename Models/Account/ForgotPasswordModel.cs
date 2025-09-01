using System.ComponentModel.DataAnnotations;

namespace dotnet_store.Models;

public class ForgotPasswordModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = null!;
}


