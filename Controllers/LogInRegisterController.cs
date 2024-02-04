﻿using JoggingTrackerAPI.Data;
using JoggingTrackerAPI.Data.Entities;
using JoggingTrackerAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace JoggingTrackerAPI.Controllers;

[ApiController]
[Route("login-register")]
public class LogInRegisterController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly UserManager<UserEntity> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _dbContext;
    public LogInRegisterController(
        IConfiguration config,
        UserManager<UserEntity> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext dbContext)
    {
        _config = config;
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
        IntializeUser();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginModel model)
    {
        var user = await _userManager.FindByNameAsync(model.UserName);

        if (user == null)
        {
            return Unauthorized("Invalid username or password.");
        }

        var result = await _userManager.CheckPasswordAsync(user, model.Password);

        if (result)
        {
            var token = await GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        return Unauthorized("Invalid username or password.");
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(IdentityResult), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> Register([FromBody] UserRegisterModel model)
    {
        var isAdminOrUserManagerOrNewUser = (User.IsInRole("Admin") || User.IsInRole("UserManager")) || !User.IsInRole("User");

        if (!isAdminOrUserManagerOrNewUser)
        {
            return Unauthorized();
        }

        else
        {

            var user = new UserEntity
            {
                UserName = model.UserLoginName,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                return Ok(result);
            }

            else
                return BadRequest($"Registration failed. Errors: {result.Errors.FirstOrDefault()?.Description}");

        }
    }

    private async Task<string> GenerateJwtToken(UserEntity user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task AddRoleAsync(string roleName)
    {
        var roleExists = await _roleManager.RoleExistsAsync(roleName);

        if (!roleExists)
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    private void IntializeUser()
    {
        AddRoleAsync("Admin").Wait();
        AddRoleAsync("UserManager").Wait();
        AddRoleAsync("User").Wait();
        if (_dbContext.Users.IsNullOrEmpty())
        {
            var user = new UserEntity
            {
                UserName = "Admin",
                Email = "Admin@gmail.com",
            };
            _userManager.CreateAsync(user, "Admin@20").Wait();
            _userManager.AddToRoleAsync(user, "Admin").Wait();
        }
    }
}

