using Microsoft.AspNetCore.Identity;

namespace dotnet_store.Models.Admin;

public class UserRoleViewModel
{
    public AppUser User { get; set; } = null!;
    public List<string> Roles { get; set; } = new List<string>();
}
