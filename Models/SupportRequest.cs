using System.ComponentModel.DataAnnotations;

namespace dotnet_store.Models;

public class SupportRequest
{
    public int Id { get; set; }

    [Required]
    [StringLength(256)]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(20)]
    public string Type { get; set; } = null!; // Yardim, Sikayet, Oneri

    [Required]
    [StringLength(4000)]
    public string Message { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? UserId { get; set; }
    public AppUser? User { get; set; }
}


