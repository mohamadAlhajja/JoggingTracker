using JoggingTrackerAPI.Controllers;
using JoggingTrackerAPI.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnitTest
{
    public class RoleControllerTests
    {
        private readonly RoleController _controller;
        private readonly Mock<UserManager<UserEntity>> _userManager;
        private readonly Mock<RoleManager<IdentityRole>> _roleManager;

        public RoleControllerTests()
        {
            _userManager = new Mock<UserManager<UserEntity>>(Mock.Of<IUserStore<UserEntity>>(), null, null, null, null, null, null, null, null);
            _roleManager = new Mock<RoleManager<IdentityRole>>(Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);
            _controller = new RoleController(_userManager.Object, _roleManager.Object);
        }

        [Fact]
        public async Task CreateRole_WithValidRoleName_ShouldReturnOk()
        {
            // Arrange
            var roleName = "NewRole";
            var expectedSuccessMessage = $"Role '{roleName}' created successfully.";

            _roleManager.Setup(x => x.FindByNameAsync(roleName)).ReturnsAsync((IdentityRole)null);
            _roleManager.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.CreateRole(roleName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
            Assert.Equal(expectedSuccessMessage, okResult.Value);
        }

        [Fact]
        public async Task AddRoleToUser_WithValidData_ShouldReturnOk()
        {
            // Arrange
            var userId = "1";
            var roleName = "NewRole";
            var user = new UserEntity { Id = userId, UserName = "TestUser" };

            _userManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _roleManager.Setup(x => x.RoleExistsAsync(roleName)).ReturnsAsync(false);
            _roleManager.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);
            _userManager.Setup(x => x.AddToRoleAsync(user, roleName)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.AddRoleToUser(userId, roleName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal($"User {user.UserName} assigned to role {roleName} successfully.", okResult.Value);
        }

        [Fact]
        public async Task RemoveUserFromRole_WithValidData_ShouldReturnOk()
        {
            // Arrange
            var userId = "1";
            var roleName = "ExistingRole";
            var user = new UserEntity { Id = userId, UserName = "TestUser" };

            _userManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManager.Setup(x => x.RemoveFromRoleAsync(user, roleName)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.RemoveUserFromRole(userId, roleName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal($"User with ID {userId} removed from role {roleName} successfully.", okResult.Value);
        }

        [Fact]
        public async Task RemoveUserFromRole_WithInvalidUser_ShouldReturnNotFound()
        {
            // Arrange
            var userId = "1";
            var roleName = "ExistingRole";

            _userManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((UserEntity)null);

            // Act
            var result = await _controller.RemoveUserFromRole(userId, roleName);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"User with ID {userId} not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task RemoveUserFromRole_WithFailedResult_ShouldReturnBadRequest()
        {
            // Arrange
            var userId = "1";
            var roleName = "ExistingRole";
            var user = new UserEntity { Id = userId, UserName = "TestUser" };
            var errorDescription = "Error description";

            _userManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManager.Setup(x => x.RemoveFromRoleAsync(user, roleName))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = errorDescription }));

            // Act
            var result = await _controller.RemoveUserFromRole(userId, roleName);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"Failed to remove user from role. Errors: {errorDescription}", badRequestResult.Value);
        }
    }
}
