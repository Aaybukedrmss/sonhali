using Microsoft.AspNetCore.Identity;

namespace dotnet_store.Models.Admin;

public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class RolePermission
{
    public int Id { get; set; }
    public int RoleId { get; set; } // string yerine int olmalı
    public int PermissionId { get; set; }
    
    public IdentityRole<int> Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}

public class UserRolePermissionViewModel
{
    public AppUser User { get; set; } = null!;
    public List<string> Roles { get; set; } = new List<string>();
    public List<Permission> AvailablePermissions { get; set; } = new List<Permission>();
    public List<int> UserPermissions { get; set; } = new List<int>();
}

public class RolePermissionViewModel
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public List<Permission> AvailablePermissions { get; set; } = new List<Permission>();
    public List<int> RolePermissions { get; set; } = new List<int>();
}
