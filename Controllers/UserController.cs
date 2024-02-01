using JoggingTrackerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace JoggingTrackerAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly UserManager<UserModel> _userManager;
    public UserController(IConfiguration config , UserManager<UserModel> userManager)
    {
        _config = config;
        _userManager = userManager;
    }

    [HttpGet(Name = "GetWeatherForecast"), Authorize]
    public UserModel Get()
    {
        return new UserModel
        {
            UserName = "Mohamad",

        };
    }

    [HttpPost, Route("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IdentityResult), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> Register([FromBody] UserRegister user)
    {
        if (user == null)
        {
            return NotFound(IdentityResult.Failed());

        }
        else
        {
            var x = new UserModel
            {
                UserName = user.UserLoginName,
                Email = user.Email
            };
            var result =await _userManager.CreateAsync(x, user.Password);
            if (result.Succeeded)
            {
                return Ok(x);
            }

            // If registration fails, return errors
            return BadRequest(result.Errors);
        }
       
    }

    [HttpPost, Route("login")]
    public string Login(UserLoginModel user)
    {
        if (user == null)
        {
            return "Invalid client request";
        }
        if (user.UserName == "m")
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
     {
                new Claim(ClaimTypes.NameIdentifier, user.UserName)
            };
            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Audience"],
              claims,
              expires: DateTime.Now.AddMinutes(15),
              signingCredentials: signinCredentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        else
        {
            return "";
        }
    }
}
