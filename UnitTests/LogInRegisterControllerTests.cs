
using JoggingTrackerAPI.Controllers;
using JoggingTrackerAPI.Data;
using JoggingTrackerAPI.Data.Entities;
using JoggingTrackerAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace UnitTest
{
    public class LogInRegisterControllerTests
    {
        private readonly LogInRegisterController _controller;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<UserManager<UserEntity>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<AppDbContext> _dbContextMock;

        public LogInRegisterControllerTests()
        {
            _configMock = new Mock<IConfiguration>();
            _userManagerMock = new Mock<UserManager<UserEntity>>(Mock.Of<IUserStore<UserEntity>>(), null, null, null, null, null, null, null, null!);
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);

            // Create the AppDbContext instance with a parameterized constructor
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            _dbContextMock = new Mock<AppDbContext>(options);

            _controller = new LogInRegisterController(_configMock.Object, _userManagerMock.Object, _roleManagerMock.Object, _dbContextMock.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                   {
                    new Claim(ClaimTypes.Name, "TestUser"),
                    new Claim(ClaimTypes.Role, "Admin")
                   }))
                }
            };
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnToken()
        {
            // Arrange
            var model = new UserLoginModel { UserName = "TestUser", Password = "TestPassword" };
            var user = new UserEntity { UserName = "TestUser" };
            var roles = new List<string> { "Admin" };

            _userManagerMock.Setup(x => x.FindByNameAsync(model.UserName)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, model.Password)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(roles);
            _configMock.Setup(x => x["Jwt:Key"]).Returns("testKeytestKeytestKeytestKeytestKey");

            // Act
            var result = await _controller.Login(model);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var model = new UserLoginModel { UserName = "InvalidUser", Password = "InvalidPassword" };

            _userManagerMock.Setup(x => x.FindByNameAsync(model.UserName)).ReturnsAsync((UserEntity)null);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid username or password.", unauthorizedResult.Value);
        }
    }
}
