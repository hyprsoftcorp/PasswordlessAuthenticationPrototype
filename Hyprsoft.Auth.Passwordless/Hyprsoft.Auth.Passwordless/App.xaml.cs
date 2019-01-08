using Hyprsoft.Auth.Passwordless.Models;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        internal const string SettingsKeyFirstRun = "FirstRun";
        internal const string SettingsKeyBearerToken = "BearerToken";

        #endregion

        #region Methods

        protected override async void OnStart()
        {
            AppCenter.Start("android=50c2eb73-d9dd-48a0-a96e-8b51be1ad56c;ios=34c1ff56-9d8d-4a53-a7a5-9c2031dc0843", typeof(Analytics), typeof(Crashes));

            // If this is our first run clear our secure storage because iOS can sync settings to iCloud.
            if (Preferences.Get(SettingsKeyFirstRun, true))
            {
                SecureStorage.RemoveAll();
                Preferences.Set(SettingsKeyFirstRun, false);
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
            if (!String.IsNullOrWhiteSpace(await SecureStorage.GetAsync(SettingsKeyBearerToken)))
                return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"[{nameof(AuthenticateAsync)}] - UserId: '{request.Id}', Token: '{request.Token}'.");
                var response = await _httpClient.PostAsync($"api/auth/token",
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    var payload = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new { Token = String.Empty });
                    await SecureStorage.SetAsync(SettingsKeyBearerToken, payload.Token);
                    Analytics.TrackEvent(nameof(AuthenticateAsync), new Dictionary<string, string> { { "UserId", request.Id } });
                    await GetUserProfileAsync(payload.Token);
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
            var bearerToken = await SecureStorage.GetAsync(SettingsKeyBearerToken);
            if (String.IsNullOrWhiteSpace(bearerToken))
                MainPage = new FeedbackPage("Hmmm, it looks like we are missing your invitaton to use this app.  Please check your email or request an invite.");
            else
                await GetUserProfileAsync(bearerToken);
        }

        private async Task GetUserProfileAsync(string bearerToken)
        {
            string feedback;
            try
            {
                System.Diagnostics.Debug.WriteLine($"[{nameof(GetUserProfileAsync)}] - Token: '{bearerToken}'.");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                var response = await _httpClient.GetAsync($"api/profile/me");
                if (response.IsSuccessStatusCode)
                {
                    var me = JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
                    feedback = $"Greetings {me.Name}.  You're authenticated using '{me.Email}'.  Thanks for trying out our password-less authentication prototype.";
                }
                else
                {
                    Analytics.TrackEvent(nameof(GetUserProfileAsync), new Dictionary<string, string> { { "Status", "Unauthorized." } });
                    feedback = "Oops, you're not authorized to use this app.";
                }
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
