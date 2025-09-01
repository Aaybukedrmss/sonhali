using System.ComponentModel.DataAnnotations;

namespace dotnet_store.Models;

public class LoginLog
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    [StringLength(256)]
    public string? UsernameOrEmail { get; set; }

    public bool IsSuccess { get; set; }

    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    [StringLength(64)]
    public string? IpAddress { get; set; }

    [StringLength(512)]
    public string? UserAgent { get; set; }

    [StringLength(512)]
    public string? ErrorMessage { get; set; }

    public AppUser? User { get; set; }
}


