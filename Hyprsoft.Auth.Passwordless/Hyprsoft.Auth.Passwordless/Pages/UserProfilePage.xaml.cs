using Hyprsoft.Auth.Passwordless.Models;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Hyprsoft.Auth.Passwordless
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserProfilePage : BaseContentPage
    {
        #region Constructors

        public UserProfilePage()
        {
            InitializeComponent();
            Title = "User Profile";
        }

        public UserProfilePage(UserProfile user) : this()
        {
            Feedback = $"You're {user.Name} and you're authenticated using '{user.Email}'.";
        }

        #endregion

        #region Properties

        private string _feedback;
        public string Feedback
        {
            get { return _feedback; }
            set
            {
                _feedback = value;
                OnPropertyChanged();
            }
        }

        public Command ShareCommand => new Command(() =>
        {
            var body = $"Tired of trying to remember all your usernames and passwords for your apps?  Try this password-less authentication app out.  No username or password required.  I believe this is how authentication should work on all apps.\n\nHelp spread the word and let's do away with usernames and passwords forever.\n\nYou can download the '{SharedSettings.AppName}' app from the appropriate store:\n\nGoogle Play Store\n{SharedSettings.AppStoreGoogleUri}\n\nApple App Store\n{SharedSettings.AppStoreAppleUri}";
            Device.OpenUri(new Uri($"mailto:?subject={SharedSettings.AppName} App&body={Uri.EscapeDataString(body)}"));
        });

        #endregion
    }
}