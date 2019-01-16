using System.Reflection;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Hyprsoft.Auth.Passwordless
{
    public class BaseContentPage : ContentPage
    {
        #region Constructors

        public BaseContentPage()
        {
            BindingContext = this;
        }

        #endregion

        #region Properties

        public ImageSource AppIcon => ImageSource.FromResource("Hyprsoft.Auth.Passwordless.logo.png", typeof(SplashPage).GetTypeInfo().Assembly);

        public string AppVersion => $"Version: {AppInfo.VersionString}";

        public Command RequestInviteCommand => new Command(async () => await Navigation.PushAsync(new InviteRequestPage()));

        #endregion
    }
}