using Hyprsoft.Auth.Passwordless.Models;
using Hyprsoft.Auth.Passwordless.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Hyprsoft.Auth.Passwordless.Tests
{
    [TestClass]
    public class WebTests : IDisposable
    {
        #region Constructors

        public WebTests()
        {
            var builder = new WebHostBuilder()
                .UseContentRoot(@"../../../../Hyprsoft.Auth.Passwordless.Web")
                .UseEnvironment("Test")
                .UseStartup<Startup>();

            WebServer = new TestServer(builder) { BaseAddress = new Uri("https://localhost:44318") };
        }

        #endregion

        #region Properties

        public string TestUserName => "Test User";

        public string TestUserEmail => "unittest@hyprsoft.com";

        public TestServer WebServer { get; }

        #endregion

        #region Methods

        [TestMethod]
        public async Task StandardWorkflow()
        {
            using (var client = WebServer.CreateClient())
            {
                // Invite
                var authenticationRequest = await GetInviteAndValidateAsync(client);

                // Authenticate
                var authenticationResponse = await AuthenticateAndValidateAsync(client, authenticationRequest);

                // User Profile
                await GetUserProfileAndValidateAsync(client, authenticationResponse);
            }   // using http client.
        }

        [TestMethod]
        public async Task BadRequests()
        {
            using (var client = WebServer.CreateClient())
            {
                // Invite
                var response = await client.PostAsync("api/auth/invite", new StringContent(String.Empty, Encoding.UTF8, "application/json"));
                Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.BadRequest);

                // Authenticate
                response = await client.PostAsync("api/auth/token", new StringContent(String.Empty, Encoding.UTF8, "application/json"));
                Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.BadRequest);

                // Invalid user.
                var authenticationRequest = new AuthenticationRequest { Id = Guid.NewGuid().ToString(), Token = "fake" };
                response = await client.PostAsync("api/auth/token", new StringContent(JsonConvert.SerializeObject(authenticationRequest), Encoding.UTF8, "application/json"));
                Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.Unauthorized);

                // Bad principal from access token
                var authenticationResponse = new AuthenticationResponse { AccessToken = "fake", RefreshToken = "fake" };
                response = await client.PostAsync("api/auth/refresh", new StringContent(JsonConvert.SerializeObject(authenticationResponse), Encoding.UTF8, "application/json"));
                Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.Unauthorized);

                // User Profile (no authentication)
                response = await client.GetAsync("api/profile/me");
                Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
            }   // using http client.
        }

        [TestMethod]
        public async Task InviteDisabledUser()
        {
            var userManager = WebServer.Host.Services.GetRequiredService<UserManager<PasswordlessAuthIdentityUser>>();
            var user = await userManager.FindByEmailAsync(TestUserEmail);
            if (user == null)
                await userManager.CreateAsync(new PasswordlessAuthIdentityUser { UserName = TestUserEmail, Email = TestUserEmail, Name = TestUserName, IsEnabled = false });
            else
            {
                user.IsEnabled = false;
                await userManager.UpdateAsync(user);
            }
            using (var client = WebServer.CreateClient())
            {
                var invitation = new InvitationRequest { Name = TestUserName, Email = TestUserEmail };
                var response = await client.PostAsync("api/auth/invite", new StringContent(JsonConvert.SerializeObject(invitation), Encoding.UTF8, "application/json"));
                Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.Forbidden);
            }   // using http client.
        }

        [TestMethod]
        public async Task RefreshWorkfow()
        {
            using (var client = WebServer.CreateClient())
            {
                // Invite
                var authenticationRequest = await GetInviteAndValidateAsync(client);

                // Authenticate
                var authenticationResponse = await AuthenticateAndValidateAsync(client, authenticationRequest);

                // User Profile
                await GetUserProfileAndValidateAsync(client, authenticationResponse);

                // Wait for our access token to expire
                var authenticationService = WebServer.Host.Services.GetRequiredService<AuthenticationService>();
                await Task.Delay(authenticationService.Options.BearerAccessTokenLifespan);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResponse.AccessToken);
                var response = await client.GetAsync("api/profile/me");
                Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
                Assert.IsTrue(response.Headers.Contains("Token-Expired"));

                // Refresh
                response = await client.PostAsync("api/auth/refresh", new StringContent(JsonConvert.SerializeObject(authenticationResponse), Encoding.UTF8, "application/json"));
                Assert.IsTrue(response.IsSuccessStatusCode);
                authenticationResponse = JsonConvert.DeserializeObject<AuthenticationResponse>(await response.Content.ReadAsStringAsync());
                Assert.IsNotNull(authenticationResponse);

                // User Profile
                await GetUserProfileAndValidateAsync(client, authenticationResponse);

                // Wait for our refresh token to expire
                await Task.Delay(authenticationService.Options.BearerRefreshTokenLifespan);
                response = await client.PostAsync("api/auth/refresh", new StringContent(JsonConvert.SerializeObject(authenticationResponse), Encoding.UTF8, "application/json"));
                Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.Forbidden);
            }   // using http client.
        }

        public void Dispose()
        {
            WebServer?.Dispose();
        }

        private async Task<AuthenticationRequest> GetInviteAndValidateAsync(HttpClient client)
        {
            var invitation = new InvitationRequest { Name = TestUserName, Email = TestUserEmail };
            var response = await client.PostAsync("api/auth/invite", new StringContent(JsonConvert.SerializeObject(invitation), Encoding.UTF8, "application/json"));
            Assert.IsTrue(response.IsSuccessStatusCode);
            var authenticationRequest = JsonConvert.DeserializeObject<AuthenticationRequest>(await response.Content.ReadAsStringAsync());
            Assert.IsNotNull(authenticationRequest);
            Assert.IsFalse(String.IsNullOrWhiteSpace(authenticationRequest.Id));
            Assert.IsFalse(String.IsNullOrWhiteSpace(authenticationRequest.Token));

            return authenticationRequest;
        }

        private async Task<AuthenticationResponse> AuthenticateAndValidateAsync(HttpClient client, AuthenticationRequest authenticationRequest)
        {
            var response = await client.PostAsync("api/auth/token", new StringContent(JsonConvert.SerializeObject(authenticationRequest), Encoding.UTF8, "application/json"));
            Assert.IsTrue(response.IsSuccessStatusCode);
            var authenticationResponse = JsonConvert.DeserializeObject<AuthenticationResponse>(await response.Content.ReadAsStringAsync());
            Assert.IsNotNull(authenticationResponse);
            Assert.IsFalse(String.IsNullOrWhiteSpace(authenticationResponse.AccessToken));
            Assert.IsFalse(String.IsNullOrWhiteSpace(authenticationResponse.RefreshToken));

            return authenticationResponse;
        }

        private async Task GetUserProfileAndValidateAsync(HttpClient client, AuthenticationResponse authenticationResponse)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResponse.AccessToken);
            var response = await client.GetAsync("api/profile/me");
            Assert.IsTrue(response.IsSuccessStatusCode);
            var userProfile = JsonConvert.DeserializeObject<UserProfile>(await response.Content.ReadAsStringAsync());
            Assert.IsNotNull(userProfile);
            Assert.AreEqual(TestUserName, userProfile.Name);
            Assert.AreEqual(TestUserEmail, userProfile.Email);
        }

        #endregion
    }
}
