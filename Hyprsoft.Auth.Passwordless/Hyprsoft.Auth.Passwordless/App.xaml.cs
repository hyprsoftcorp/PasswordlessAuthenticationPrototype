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

        private const string PreferencesKeyFirstRun = "FirstRun";

        private const string StorageKeyAccessToken = "AccessToken";
        private const string StorageKeyRefreshToken = "RefreshToken";

        #endregion

        #region Constructors

        public App()
        {
            InitializeComponent();
            MainPage = new SplashPage();
        }

        #endregion

        #region Properties

        internal static readonly HttpClient HttpClient = new HttpClient { BaseAddress = SharedSettings.AppWebUri };

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

        protected override async void OnAppLinkRequestReceived(Uri uri)
        {
            base.OnAppLinkRequestReceived(uri);

            var accessToken = await SecureStorage.GetAsync(StorageKeyAccessToken);
            var refreshToken = await SecureStorage.GetAsync(StorageKeyRefreshToken);

            if (String.IsNullOrWhiteSpace(accessToken) || String.IsNullOrWhiteSpace(refreshToken))
            {
                var queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
                await AuthenticateAsync(new AuthenticationRequest { Id = queryString["id"], Token = queryString["token"] });
            }   // already authenticated?
        }

        private async Task InitializeAsync()
        {
            var accessToken = await SecureStorage.GetAsync(StorageKeyAccessToken);
            var refreshToken = await SecureStorage.GetAsync(StorageKeyRefreshToken);

            if (String.IsNullOrWhiteSpace(accessToken) || String.IsNullOrWhiteSpace(refreshToken))
                MainPage = new NavigationPage(new InviteNeededPage());
            else
                await GetUserProfileAsync(accessToken, refreshToken);
        }

        private async Task AuthenticateAsync(AuthenticationRequest request)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[{nameof(AuthenticateAsync)}] - UserId: '{request.Id}', Token: '{request.Token}'.");
                var response = await HttpClient.PostAsync($"api/auth/token",
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    var payload = JsonConvert.DeserializeObject<AuthenticationResponse>(await response.Content.ReadAsStringAsync());
                    await SecureStorage.SetAsync(StorageKeyAccessToken, payload.AccessToken);
                    await SecureStorage.SetAsync(StorageKeyRefreshToken, payload.RefreshToken);
                    Analytics.TrackEvent(nameof(AuthenticateAsync), new Dictionary<string, string> { { "Status", $"[SUCCESS] UserId: '{request.Id}'" } });
                    await GetUserProfileAsync(payload.AccessToken, payload.RefreshToken);
                }
                else
                {
                    Analytics.TrackEvent(nameof(AuthenticateAsync), new Dictionary<string, string> { { "Status", "[FAILED] Invitation expired or invalid." } });
                    MainPage = new NavigationPage(new InviteInvalidPage());
                }
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                await MainPage.DisplayAlert("Error", $"We appologize, but it looks like something bad happened.  Please try again later.  Details: {ex.Message}.", "Dismiss");
            }
        }

        private async Task GetUserProfileAsync(string accessToken, string refreshToken)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[{nameof(GetUserProfileAsync)}] - Token: '{accessToken}', Refresh: '{refreshToken}'.");
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await HttpClient.GetAsync($"api/profile/me");
                if (response.IsSuccessStatusCode)
                {
                    var user = JsonConvert.DeserializeObject<UserProfile>(await response.Content.ReadAsStringAsync());
                    MainPage = new NavigationPage(new UserProfilePage(user));
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized && response.Headers.Contains("Token-Expired"))
                {
                    response = await HttpClient.PostAsync($"api/auth/refresh",
                        new StringContent(JsonConvert.SerializeObject(new AuthenticationResponse { AccessToken = accessToken, RefreshToken = refreshToken }), Encoding.UTF8, "application/json"));
                    if (response.IsSuccessStatusCode)
                    {
                        var payload = JsonConvert.DeserializeObject<AuthenticationResponse>(await response.Content.ReadAsStringAsync());
                        await SecureStorage.SetAsync(StorageKeyAccessToken, payload.AccessToken);
                        await SecureStorage.SetAsync(StorageKeyRefreshToken, payload.RefreshToken);
                        await GetUserProfileAsync(payload.AccessToken, payload.RefreshToken);
                    }   // successful response code?
                    else
                    {
                        Analytics.TrackEvent(nameof(GetUserProfileAsync), new Dictionary<string, string> { { "Status", $"[FAILED] Refresh failed.  Reason: '{response.ReasonPhrase}'." } });
                        MainPage = new NavigationPage(new AccessDeniedPage());
                    }
                }
                else
                {
                    Analytics.TrackEvent(nameof(GetUserProfileAsync), new Dictionary<string, string> { { "Status", $"[FAILED] Reason: '{response.ReasonPhrase}'." } });
                    MainPage = new NavigationPage(new AccessDeniedPage());
                }   // successful response code?
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                await MainPage.DisplayAlert("Error", $"We appologize, but it looks like something bad happened.  Please try again later.  Details: {ex.Message}.", "Dismiss");
            }
        }
        #endregion
    }
}
