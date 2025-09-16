using Microsoft.EntityFrameworkCore;

namespace dotnet_store.Models;

[Keyless]
public class ResimUrun
{
    public long Isbn { get; set; }
    public string? Url { get; set; }
    public string? Images { get; set; }
}


