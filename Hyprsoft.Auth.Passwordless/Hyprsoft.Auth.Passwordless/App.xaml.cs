using Hyprsoft.Auth.Passwordless.Models;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Hyprsoft.Auth.Passwordless
{
    public partial class App : Application
    {
        #region Fields

        private readonly HttpClient _httpClient;

        #endregion

        #region Constructors

        public App()
        {
            InitializeComponent();

            _httpClient = new HttpClient { BaseAddress = SharedSettings.AppWebUri };
            MainPage = new SplashPage();
        }

        #endregion

        #region Properties

        internal const string PreferencesKeyFirstRun = "FirstRun";

        internal const string StorageKeyAccessToken = "AccessToken";
        internal const string StorageKeyRefreshToken = "RefreshToken";

        #endregion

        #region Methods

        protected override async void OnStart()
        {
            AppCenter.Start("android=50c2eb73-d9dd-48a0-a96e-8b51be1ad56c;ios=34c1ff56-9d8d-4a53-a7a5-9c2031dc0843", typeof(Analytics), typeof(Crashes));

            // If this is our first run clear our secure storage because iOS can sync/remember settings to iCloud.
            if (Preferences.Get(PreferencesKeyFirstRun, true))
            {
                SecureStorage.RemoveAll();
                Preferences.Set(PreferencesKeyFirstRun, false);
            }
            await InitializeAsync();
        }

        protected override async void OnResume()
        {
            await InitializeAsync();
        }

        public async Task AuthenticateAsync(AuthenticationRequest request)
        {
            // Ignore auth requests if we are already authenticated.
            if (!String.IsNullOrWhiteSpace(await SecureStorage.GetAsync(StorageKeyAccessToken)))
                return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"[{nameof(AuthenticateAsync)}] - UserId: '{request.Id}', Token: '{request.Token}'.");
                var response = await _httpClient.PostAsync($"api/auth/token",
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    var payload = JsonConvert.DeserializeObject<AuthenticationData>(await response.Content.ReadAsStringAsync());
                    await SecureStorage.SetAsync(StorageKeyAccessToken, payload.AccessToken);
                    await SecureStorage.SetAsync(StorageKeyRefreshToken, payload.RefreshToken);
                    Analytics.TrackEvent(nameof(AuthenticateAsync), new Dictionary<string, string> { { "UserId", request.Id } });
                    await GetUserProfileAsync(payload.AccessToken, payload.RefreshToken);
                    return;
                }
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                MainPage = new FeedbackPage($"Uh oh, we have a problem.  Please try again later.  Details: {ex.Message}.");
            }
            Analytics.TrackEvent(nameof(AuthenticateAsync), new Dictionary<string, string> { { "Status", "Invitation expired or invalid." } });
            MainPage = new FeedbackPage("The invitation to use this app has expired or is invalid.  Please request a new invite.");
        }

        private async Task InitializeAsync()
        {
            var accessToken = await SecureStorage.GetAsync(StorageKeyAccessToken);
            var refreshToken = await SecureStorage.GetAsync(StorageKeyRefreshToken);

            if (String.IsNullOrWhiteSpace(accessToken) || String.IsNullOrWhiteSpace(refreshToken))
                MainPage = new FeedbackPage("Hmmm, it looks like we are missing your invitaton to use this app.  Please check your email or request an invite.");
            else
                await GetUserProfileAsync(accessToken, refreshToken);
        }

        private async Task GetUserProfileAsync(string accessToken, string refreshToken)
        {
            string feedback = "Oops, you're not authorized to use this app.";
            try
            {
                System.Diagnostics.Debug.WriteLine($"[{nameof(GetUserProfileAsync)}] - Token: '{accessToken}'.");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await _httpClient.GetAsync($"api/profile/me");
                if (response.IsSuccessStatusCode)
                {
                    var me = JsonConvert.DeserializeObject<UserProfile>(await response.Content.ReadAsStringAsync());
                    feedback = $"Greetings {me.Name}.  You're authenticated using '{me.Email}'.  Thanks for trying out our password-less authentication prototype.";
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized && response.Headers.Contains("Token-Expired"))
                {
                    response = await _httpClient.PostAsync($"api/auth/refresh",
                        new StringContent(JsonConvert.SerializeObject(new AuthenticationData { AccessToken = accessToken, RefreshToken = refreshToken }), Encoding.UTF8, "application/json"));
                    if (response.IsSuccessStatusCode)
                    {
                        var payload = JsonConvert.DeserializeObject<AuthenticationData>(await response.Content.ReadAsStringAsync());
                        await SecureStorage.SetAsync(StorageKeyAccessToken, payload.AccessToken);
                        await SecureStorage.SetAsync(StorageKeyRefreshToken, payload.RefreshToken);
                        await GetUserProfileAsync(payload.AccessToken, payload.RefreshToken);
                        return;
                    }
                    Analytics.TrackEvent(nameof(GetUserProfileAsync), new Dictionary<string, string> { { "Status", "Unauthorized." } });
                }
                else
                    Analytics.TrackEvent(nameof(GetUserProfileAsync), new Dictionary<string, string> { { "Status", "Unauthorized." } });
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                feedback = $"Uh oh, we have a problem.  Please try again later.  Details: {ex.Message}.";
            }
            MainPage = new FeedbackPage(feedback);
        }

        protected override async void OnAppLinkRequestReceived(Uri uri)
        {
            base.OnAppLinkRequestReceived(uri);
            var queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
            await AuthenticateAsync(new AuthenticationRequest { Id = queryString["id"], Token = queryString["token"] });
        }

        #endregion
    }
}
