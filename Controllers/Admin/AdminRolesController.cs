using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dotnet_store.Models;
using dotnet_store.Models.Admin;

namespace dotnet_store.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/roles")] 
public class AdminRolesController : Controller
{
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly DataContext _dbContext;

    public AdminRolesController(RoleManager<IdentityRole<int>> roleManager, UserManager<AppUser> userManager, DataContext dbContext)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    // Kullanıcı listesi için yeni action
    [HttpGet("users")]
    public async Task<IActionResult> Users()
    {
        var users = _userManager.Users.ToList();
        var userRoles = new List<UserRoleViewModel>();
        
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userRoles.Add(new UserRoleViewModel
            {
                User = user,
                Roles = roles.ToList()
            });
        }
        
        return View("~/Views/Admin/Roles/Users.cshtml", userRoles);
    }

    // Rol kaldırma için yeni action
    [HttpPost("remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveRole(string userId, string role)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role))
        {
            TempData["ErrorMessage"] = "Kullanıcı ve rol zorunludur";
            return RedirectToAction("Users");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Kullanıcı bulunamadı";
            return RedirectToAction("Users");
        }

        var result = await _userManager.RemoveFromRoleAsync(user, role);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = 
            result.Succeeded ? "Rol kaldırıldı" : string.Join(", ", result.Errors.Select(e => e.Description));
        
        return RedirectToAction("Users");
    }

    // Rol silme için yeni action
    [HttpPost("delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRole(string roleId)
    {
        if (string.IsNullOrWhiteSpace(roleId))
        {
            TempData["ErrorMessage"] = "Rol ID zorunludur";
            return RedirectToAction("Index");
        }

        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            TempData["ErrorMessage"] = "Rol bulunamadı";
            return RedirectToAction("Index");
        }

        // Admin rolünü silmeyi engelle
        if (role.Name == "Admin")
        {
            TempData["ErrorMessage"] = "Admin rolü silinemez";
            return RedirectToAction("Index");
        }

        var result = await _roleManager.DeleteAsync(role);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = 
            result.Succeeded ? "Rol silindi" : string.Join(", ", result.Errors.Select(e => e.Description));
        
        return RedirectToAction("Index");
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var roles = _roleManager.Roles.ToList();
        return View("~/Views/Admin/Roles/Index.cshtml", roles);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["ErrorMessage"] = "Rol adı zorunludur";
            return RedirectToAction("Index");
        }
        if (!await _roleManager.RoleExistsAsync(name))
        {
            var result = await _roleManager.CreateAsync(new IdentityRole<int>(name));
            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded ? "Rol oluşturuldu" : string.Join(", ", result.Errors.Select(e => e.Description));
        }
        else
        {
            TempData["ErrorMessage"] = "Rol zaten mevcut";
        }
        return RedirectToAction("Index");
    }

    [HttpPost("assign")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(string usernameOrEmail, string role)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(role))
        {
            TempData["ErrorMessage"] = "Kullanıcı ve rol zorunludur";
            return RedirectToAction("Index");
        }

        var user = await _userManager.FindByNameAsync(usernameOrEmail);
        user ??= await _userManager.FindByEmailAsync(usernameOrEmail);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Kullanıcı bulunamadı";
            return RedirectToAction("Index");
        }
        if (!await _roleManager.RoleExistsAsync(role))
        {
            TempData["ErrorMessage"] = "Rol bulunamadı";
            return RedirectToAction("Index");
        }
        var result = await _userManager.AddToRoleAsync(user, role);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded ? "Rol atandı" : string.Join(", ", result.Errors.Select(e => e.Description));
        return RedirectToAction("Index");
    }

    // İzin sistemi için yeni action'lar
    [HttpGet("permissions")]
    public async Task<IActionResult> Permissions()
    {
        var permissions = await _dbContext.Permissions.ToListAsync();
        return View("~/Views/Admin/Roles/Permissions.cshtml", permissions);
    }

    [HttpGet("role-permissions/{roleId}")]
    public async Task<IActionResult> RolePermissions(int roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            TempData["ErrorMessage"] = "Rol bulunamadı";
            return RedirectToAction("Index");
        }

        var availablePermissions = await _dbContext.Permissions.ToListAsync();
        var rolePermissions = await _dbContext.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        var viewModel = new RolePermissionViewModel
        {
            RoleId = roleId.ToString(),
            RoleName = role.Name ?? "",
            AvailablePermissions = availablePermissions,
            RolePermissions = rolePermissions
        };

        return View("~/Views/Admin/Roles/RolePermissions.cshtml", viewModel);
    }

    [HttpPost("role-permissions/{roleId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRolePermissions(int roleId, List<int> permissionIds)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            TempData["ErrorMessage"] = "Rol bulunamadı";
            return RedirectToAction("Index");
        }

        // Mevcut izinleri sil
        var existingPermissions = await _dbContext.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();
        _dbContext.RolePermissions.RemoveRange(existingPermissions);

        // Yeni izinleri ekle
        if (permissionIds != null && permissionIds.Any())
        {
            var newPermissions = permissionIds.Select(permissionId => new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            }).ToList();
            _dbContext.RolePermissions.AddRange(newPermissions);
        }

        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Rol izinleri güncellendi";
        return RedirectToAction("RolePermissions", new { roleId });
    }

    [HttpGet("user-permissions/{userId}")]
    public async Task<IActionResult> UserPermissions(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            TempData["ErrorMessage"] = "Kullanıcı bulunamadı";
            return RedirectToAction("Users");
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        var availablePermissions = await _dbContext.Permissions.ToListAsync();
        
        // Kullanıcının rollerinden gelen izinleri topla
        var userPermissionIds = new List<int>();
        foreach (var roleName in userRoles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var rolePermissions = await _dbContext.RolePermissions
                    .Where(rp => rp.RoleId == role.Id)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();
                userPermissionIds.AddRange(rolePermissions);
            }
        }

        var viewModel = new UserRolePermissionViewModel
        {
            User = user,
            Roles = userRoles.ToList(),
            AvailablePermissions = availablePermissions,
            UserPermissions = userPermissionIds.Distinct().ToList()
        };

        return View("~/Views/Admin/Roles/UserPermissions.cshtml", viewModel);
    }

    [HttpPost("create-permission")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePermission(string name, string description, string category)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(category))
        {
            TempData["ErrorMessage"] = "İzin adı ve kategori zorunludur";
            return RedirectToAction("Permissions");
        }

        var permission = new Permission
        {
            Name = name,
            Description = description ?? "",
            Category = category
        };

        _dbContext.Permissions.Add(permission);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "İzin başarıyla eklendi";
        return RedirectToAction("Permissions");
    }

    // Test verilerini ekleme action'ı
    [HttpPost("add-test-data")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTestData()
    {
        try
        {
            // Test izinleri oluştur
            var testPermissions = new List<Permission>
            {
                new Permission { Name = "Ürün Ekleme", Description = "Yeni ürün ekleyebilir", Category = "Ürün Yönetimi" },
                new Permission { Name = "Ürün Düzenleme", Description = "Mevcut ürünleri düzenleyebilir", Category = "Ürün Yönetimi" },
                new Permission { Name = "Ürün Silme", Description = "Ürünleri silebilir", Category = "Ürün Yönetimi" },
                new Permission { Name = "Ürün Görüntüleme", Description = "Ürünleri görüntüleyebilir", Category = "Ürün Yönetimi" },
                new Permission { Name = "Sipariş Görüntüleme", Description = "Siparişleri görüntüleyebilir", Category = "Sipariş Yönetimi" },
                new Permission { Name = "Sipariş Düzenleme", Description = "Siparişleri düzenleyebilir", Category = "Sipariş Yönetimi" },
                new Permission { Name = "Sipariş İptal Etme", Description = "Siparişleri iptal edebilir", Category = "Sipariş Yönetimi" },
                new Permission { Name = "Kullanıcı Ekleme", Description = "Yeni kullanıcı ekleyebilir", Category = "Kullanıcı Yönetimi" },
                new Permission { Name = "Kullanıcı Düzenleme", Description = "Kullanıcıları düzenleyebilir", Category = "Kullanıcı Yönetimi" },
                new Permission { Name = "Kullanıcı Silme", Description = "Kullanıcıları silebilir", Category = "Kullanıcı Yönetimi" },
                new Permission { Name = "Rol Oluşturma", Description = "Yeni rol oluşturabilir", Category = "Rol Yönetimi" },
                new Permission { Name = "Rol Atama", Description = "Kullanıcılara rol atayabilir", Category = "Rol Yönetimi" },
                new Permission { Name = "Rol Silme", Description = "Rolleri silebilir", Category = "Rol Yönetimi" },
                new Permission { Name = "Site Ayarları", Description = "Site ayarlarını değiştirebilir", Category = "Site Yönetimi" },
                new Permission { Name = "Slider Yönetimi", Description = "Slider'ları yönetebilir", Category = "Site Yönetimi" },
                new Permission { Name = "Kategori Yönetimi", Description = "Kategorileri yönetebilir", Category = "Site Yönetimi" },
                new Permission { Name = "Destek Talepleri Görüntüleme", Description = "Destek taleplerini görüntüleyebilir", Category = "Destek Yönetimi" },
                new Permission { Name = "Destek Talepleri Yanıtlama", Description = "Destek taleplerini yanıtlayabilir", Category = "Destek Yönetimi" },
                new Permission { Name = "Destek Talepleri Silme", Description = "Destek taleplerini silebilir", Category = "Destek Yönetimi" }
            };

            // İzinleri veritabanına ekle
            foreach (var permission in testPermissions)
            {
                if (!await _dbContext.Permissions.AnyAsync(p => p.Name == permission.Name))
                {
                    _dbContext.Permissions.Add(permission);
                }
            }

            await _dbContext.SaveChangesAsync();

            // Test rolleri oluştur
            var testRoles = new List<string> { "Editor", "Manager", "Support", "Moderator" };
            foreach (var roleName in testRoles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }

            // Rollere izin atama
            var permissions = await _dbContext.Permissions.ToListAsync();
            var roles = await _roleManager.Roles.ToListAsync();

            // Editor rolüne ürün yönetimi izinleri
            var editorRole = roles.FirstOrDefault(r => r.Name == "Editor");
            if (editorRole != null)
            {
                var editorPermissions = permissions.Where(p => 
                    p.Category == "Ürün Yönetimi" && 
                    (p.Name == "Ürün Ekleme" || p.Name == "Ürün Düzenleme" || p.Name == "Ürün Görüntüleme")
                ).ToList();

                foreach (var perm in editorPermissions)
                {
                    if (!await _dbContext.RolePermissions.AnyAsync(rp => rp.RoleId == editorRole.Id && rp.PermissionId == perm.Id))
                    {
                        _dbContext.RolePermissions.Add(new RolePermission
                        {
                            RoleId = editorRole.Id,
                            PermissionId = perm.Id
                        });
                    }
                }
            }

            // Manager rolüne sipariş yönetimi izinleri
            var managerRole = roles.FirstOrDefault(r => r.Name == "Manager");
            if (managerRole != null)
            {
                var managerPermissions = permissions.Where(p => 
                    p.Category == "Sipariş Yönetimi"
                ).ToList();

                foreach (var perm in managerPermissions)
                {
                    if (!await _dbContext.RolePermissions.AnyAsync(rp => rp.RoleId == managerRole.Id && rp.PermissionId == perm.Id))
                    {
                        _dbContext.RolePermissions.Add(new RolePermission
                        {
                            RoleId = managerRole.Id,
                            PermissionId = perm.Id
                        });
                    }
                }
            }

            // Support rolüne destek yönetimi izinleri
            var supportRole = roles.FirstOrDefault(r => r.Name == "Support");
            if (supportRole != null)
            {
                var supportPermissions = permissions.Where(p => 
                    p.Category == "Destek Yönetimi"
                ).ToList();

                foreach (var perm in supportPermissions)
                {
                    if (!await _dbContext.RolePermissions.AnyAsync(rp => rp.RoleId == supportRole.Id && rp.PermissionId == perm.Id))
                    {
                        _dbContext.RolePermissions.Add(new RolePermission
                        {
                            RoleId = supportRole.Id,
                            PermissionId = perm.Id
                        });
                    }
                }
            }

            // Moderator rolüne genel izinler
            var moderatorRole = roles.FirstOrDefault(r => r.Name == "Moderator");
            if (moderatorRole != null)
            {
                var moderatorPermissions = permissions.Where(p => 
                    p.Category == "Ürün Yönetimi" || 
                    p.Category == "Kullanıcı Yönetimi" ||
                    p.Category == "Site Yönetimi"
                ).ToList();

                foreach (var perm in moderatorPermissions)
                {
                    if (!await _dbContext.RolePermissions.AnyAsync(rp => rp.RoleId == moderatorRole.Id && rp.PermissionId == perm.Id))
                    {
                        _dbContext.RolePermissions.Add(new RolePermission
                        {
                            RoleId = moderatorRole.Id,
                            PermissionId = perm.Id
                        });
                    }
                }
            }

            // Admin rolüne özel izinler (tüm izinler değil, sadece seçilenler)
            var adminRole = roles.FirstOrDefault(r => r.Name == "Admin");
            if (adminRole != null)
            {
                // Admin rolüne sadece kritik izinleri ver
                var adminPermissions = permissions.Where(p => 
                    p.Name == "Rol Oluşturma" || 
                    p.Name == "Rol Atama" || 
                    p.Name == "Rol Silme" ||
                    p.Name == "Kullanıcı Ekleme" ||
                    p.Name == "Kullanıcı Düzenleme" ||
                    p.Name == "Kullanıcı Silme" ||
                    p.Name == "Site Ayarları"
                ).ToList();

                foreach (var perm in adminPermissions)
                {
                    if (!await _dbContext.RolePermissions.AnyAsync(rp => rp.RoleId == adminRole.Id && rp.PermissionId == perm.Id))
                    {
                        _dbContext.RolePermissions.Add(new RolePermission
                        {
                            RoleId = adminRole.Id,
                            PermissionId = perm.Id
                        });
                    }
                }
            }

            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Test verileri başarıyla eklendi! {testPermissions.Count} izin, {testRoles.Count} rol ve rol-izin atamaları oluşturuldu.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Test verileri eklenirken hata oluştu: {ex.Message}";
            return RedirectToAction("Index");
        }
    }
}


