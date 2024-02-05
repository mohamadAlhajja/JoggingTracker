using JoggingTrackerAPI;
using JoggingTrackerAPI.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests.E2E
{
    [Collection("User tests")]
    public class UserJoggingTrackerE2ETests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private static string Token = default!;
        private static string _userId = default!;
        public UserJoggingTrackerE2ETests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            if (!string.IsNullOrEmpty(Token))
            {
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            }
        }

        [Fact]
        public async Task UserCanManageHisOwnRecords()
        {
            await UserCanRegister();
            await RegisterdUserShouldBeAbleToLogIn();
            await UserUnAutherizedToAttachUserToRole();
            await UserCanCreateJoggingTrackerRecordForhimSelf();
            await UserCanGenerateReportForHimSelf();
            await UserCanDeleteHisJoggingTrackerRecord();
            await UserUnautherizedToDeleteAUser();
            await AdminLoginShouldReturnOk();
            await AdminWillDeleteTheRegisterUser();
        }

        private async Task AdminWillDeleteTheRegisterUser()
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            var response = await _client.DeleteAsync($"users/delete/{_userId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task AdminLoginShouldReturnOk()
        {
            // Arrange
            var model = new UserLoginModel
            {
                UserName = "Admin",
                Password = "Admin@20"
            };
            //Act
            var response = await _client.PostAsync("login-register/login", model.ToJsonContent());

            var anonymousTypeInstance = new { Token = "" };
            var result = response.DeserializeAnonymous(anonymousTypeInstance);

            Token = result.Token;
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task UserCanDeleteHisJoggingTrackerRecord()
        {
            //Arrange
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            var response = await _client.GetAsync("jogging-tracker");
            var result = response.Deserialize<IEnumerable<JoggingTrackerResource>>().ToList();
            var joggingToDelete = result.Find(jogging => jogging.UserId == _userId);

            //Act
            var deletResponse = await _client.DeleteAsync($"jogging-tracker/{joggingToDelete?.Id}");

            // Assert
            Assert.NotNull(joggingToDelete);
            Assert.Equal(HttpStatusCode.NoContent, deletResponse.StatusCode);
        }

        private async Task UserCanGenerateReportForHimSelf()
        {
            //Arrange
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

            //Act
            var response = await _client.GetAsync($"jogging-tracker/generate-report?userId={_userId}&startDate=2024-02-04T00%3A07%3A19.7776695");
            var result = response.Deserialize<WeeklyReportResource>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(50, result.Distance);
        }

        private async Task UserCanCreateJoggingTrackerRecordForhimSelf()
        {
            //Arrange
            var model = new JoggingTrackerModel
            {
                UserId = _userId,
                Date = new DateTime(2024, 02, 05),
                Distance = 50,
                Time = new TimeSpan(1, 20, 1),
                Location = "Tulkarem"
            };
            //Act
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            var response = await _client.PostAsync("jogging-tracker", model.ToJsonContent());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task UserUnautherizedToDeleteAUser()
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            var response = await _client.DeleteAsync($"users/delete/{_userId}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private async Task UserUnAutherizedToAttachUserToRole()
        {
            //Act
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            var userResponse = await _client.GetAsync("users/all-users");
            var user = userResponse.Deserialize<UserModel>();
            _userId = user.Id!;
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            var role = await _client.PutAsync($"roles/attach-role?userId={_userId}&roleName=UserManager", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, userResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, role.StatusCode);

        }

        private async Task RegisterdUserShouldBeAbleToLogIn()
        {
            // Arrange
            var model = new UserLoginModel
            {
                UserName = "User",
                Password = "User@10"
            };
            //Act
            var response = await _client.PostAsync("login-register/login", model.ToJsonContent());

            var anonymousTypeInstance = new { Token = "" };
            var result = response.DeserializeAnonymous(anonymousTypeInstance);

            Token = result.Token;
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


        private async Task UserCanRegister()
        {
            var model = new UserRegisterModel
            {
                UserLoginName = "User",
                Email = "User@gmail.com",
                Password = "User@10"
            };
            //Act
            var response = await _client.PostAsync("login-register/register", model.ToJsonContent());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
