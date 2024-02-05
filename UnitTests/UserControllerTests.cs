using JoggingTrackerAPI.Controllers;
using JoggingTrackerAPI.Data;
using JoggingTrackerAPI.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore.Query;
using JoggingTrackerAPI.Models;

namespace UnitTest
{
    public class UserControllerTests
    {
        private readonly UserController _controller;
        private readonly Mock<UserManager<UserEntity>> _userManager;

        public UserControllerTests()
        {
            // Mock UserManager
            _userManager = new Mock<UserManager<UserEntity>>(Mock.Of<IUserStore<UserEntity>>(), null, null, null, null, null, null, null, null!);

            // Create the controller with the mocked UserManager
            _controller = new UserController(_userManager.Object);

            // Set up ControllerContext with a Mock HttpContext
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
        public async Task UpdateUser_WithInvalidUserId_ShouldReturnNotFound()
        {
            // Arrange
            var userId = "invalidUserId";
            var model = new UserRegisterModel
            {
                UserLoginName = "newUserName",
                Email = "newEmail@example.com"
            };

            // Act
            var result = await _controller.UpdateUser(userId, model);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteUser_ShouldDeleteUser_WhenAuthorized()
        {
            // Arrange
            var userId = "1";
            var user = new UserEntity { Id = userId, UserName = "user1" };

            _userManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _userManager.Setup(x => x.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var message = Assert.IsType<string>(okResult.Value);
            Assert.Equal($"User with ID {userId} deleted successfully.", message);
        }
    }
}