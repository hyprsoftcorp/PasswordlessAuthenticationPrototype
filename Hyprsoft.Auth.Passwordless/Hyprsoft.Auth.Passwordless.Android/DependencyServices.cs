using Xamarin.Forms;

[assembly: Dependency(typeof(Hyprsoft.Auth.Passwordless.Droid.AppInformation))]
namespace Hyprsoft.Auth.Passwordless.Droid
{
    public class AppInformation : IAppInformation
    {
        public string Version
        {
            get { return Android.App.Application.Context.PackageManager.GetPackageInfo(Android.App.Application.Context.PackageName, 0).VersionName; }
        }
    }
}