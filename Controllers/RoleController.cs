using JoggingTrackerAPI.Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JoggingTrackerAPI.Controllers;

[ApiController]
[Route("roles")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class RoleController : ControllerBase
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleController(UserManager<UserEntity> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet("all-roles")]
    [Authorize(Roles = "Admin,UserManager")]
    public async Task<ActionResult<IEnumerable<string>>> GetAllRoles(int? skip, int? take)
    {
        var roles = (await _roleManager.Roles.Select(r => r.Name).ToListAsync()).Skip(skip ?? 0).Take(take ?? int.MaxValue);
        return Ok(roles);
    }

    [HttpPost("create-role")]
    [Authorize(Roles = "Admin,UserManager")]
    public async Task<ActionResult<string>> CreateRole(string roleName)
    {
        if (string.IsNullOrEmpty(roleName))
        {
            return BadRequest("Role name cannot be empty.");
        }

        var existingRole = await _roleManager.FindByNameAsync(roleName);
        if (existingRole != null)
        {
            return BadRequest("Role already exists.");
        }

        var newRole = new IdentityRole(roleName);
        var result = await _roleManager.CreateAsync(newRole);

        if (result.Succeeded)
        {
            return Ok($"Role '{roleName}' created successfully.");
        }
        else
        {
            return BadRequest($"Failed to create role. Errors: {string.Join(", ", result.Errors)}");
        }
    }

    [HttpPut("api/attach-role")]
    [Authorize(Roles = "Admin,UserManager")]
    public async Task<IActionResult> AddRoleToUser(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound("User not found");
        }      
        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            if (roleName == "UseManager")
            {
                await _userManager.RemoveFromRoleAsync(user, roleName);
            }
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!roleResult.Succeeded)
            {
                return BadRequest("Unable to create role");
            }
        }

        var result = await _userManager.AddToRoleAsync(user, roleName);

        if (result.Succeeded)
        {
            return Ok($"User {user.UserName} assigned to role {roleName} successfully.");
        }
        else
        {
            return BadRequest("Unable to assign role to user");
        }
    }

    [HttpPut("api/detach-role")]
    [Authorize(Roles = "Admin,UserManager")]
    public async Task<IActionResult> RemoveUserFromRole(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound($"User with ID {userId} not found.");
        }

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);

        if (result.Succeeded)
        {
            return Ok($"User with ID {userId} removed from role {roleName} successfully.");
        }
        else
        {
            return BadRequest($"Failed to remove user from role. Errors: {result.Errors.FirstOrDefault()?.Description}");
        }
    }

}

