using Foundation;
using Xamarin.Forms;

[assembly: Dependency(typeof(Hyprsoft.Auth.Passwordless.iOS.AppInformation))]
namespace Hyprsoft.Auth.Passwordless.iOS
{
    public class AppInformation : IAppInformation
    {
        public string Version
        {
            get { return NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString(); }
        }
    }
}