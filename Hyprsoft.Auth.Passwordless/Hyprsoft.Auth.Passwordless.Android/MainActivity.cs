using Android.App;
using Android.Content.PM;
using Android.Content;
using Android.OS;
using Hyprsoft.Auth.Passwordless.Models;
using System.Threading.Tasks;

namespace Hyprsoft.Auth.Passwordless.Droid
{
    [IntentFilter(new[] { Intent.ActionView },
              Categories = new[] { Intent.ActionView, Intent.CategoryDefault, Intent.CategoryBrowsable },
              DataScheme = "https",
              DataHost = "pwdlessauth.azurewebsites.net",
              DataPathPrefix = "/api/auth/token",
              AutoVerify = true)]

    [Activity(Label = SharedSettings.AppName,
        Icon = "@mipmap/icon",
        Theme = "@style/MainTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleInstance,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
            await ProcessAuthenticationRequestAsync(Intent);
        }

        protected override async void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            await ProcessAuthenticationRequestAsync(intent);
        }

        private async Task ProcessAuthenticationRequestAsync(Intent intent)
        {
            if (intent?.Data != null)
                await ((App)App.Current).AuthenticateAsync(new AuthenticationRequest { Id = intent.Data.GetQueryParameter("id"), Token = intent.Data.GetQueryParameter("token") });
        }
    }
}