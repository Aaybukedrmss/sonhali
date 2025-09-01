using System.ComponentModel.DataAnnotations;

namespace dotnet_store.Models.Account;

public class UserAddressSaveModel
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string District { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Neighborhood { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Details { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
}


