using JoggingTrackerAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JoggingTrackerAPI.Controllers;

[ApiController]
[Route("users")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UserController : ControllerBase
{
    private readonly UserManager<UserModel> _userManager;

    public UserController(UserManager<UserModel> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("users")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<UserModel>>> GetAllUsers(int? skip, int? take)
    {
        var isAdminOrUserManager = User.IsInRole("Admin") || User.IsInRole("UserManager");
        if (isAdminOrUserManager)
        {
            var users = (await _userManager.Users.ToListAsync()).Skip(skip ?? 0).Take(take ?? int.MaxValue);
            return Ok(users);

        }
        else
            return Ok(await _userManager.GetUserAsync(User));
    }

    [HttpDelete("delete/{userId}")]
    [Authorize]
    public async Task<IActionResult> DeleteUser(string userId)
    {
            var user = await _userManager.FindByIdAsync(userId);
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }
            if (roles.Contains("Admin") || roles.Contains("UserManager"))
            {
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    return Ok($"User with ID {userId} deleted successfully.");
                }
                else
                {
                    return BadRequest($"Failed to delete user. Errors: {string.Join(", ", result.Errors)}");
                }
            }
            else
                return Unauthorized("User with your role can't delete the user");
    }

    [HttpPut("update/{userId}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(string userId, [FromBody] UserRegisterModel model)
    {
        var isAdminOrUserManager = User.IsInRole("Admin") || User.IsInRole("UserManager") || (User.IsInRole("User") && model.UserLoginName == User.Identity?.Name);
        if (isAdminOrUserManager)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            user.Email = model.Email;
            user.UserName = model.UserLoginName;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok($"User with ID {userId} updated successfully.");
            }
            else
            {
                return BadRequest($"Failed to update user. Errors: {string.Join(", ", result.Errors)}");
            }
        } else
            return Unauthorized("User with your role can't update the user");
    }
}
