using System;
using System.Reflection;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Hyprsoft.Auth.Passwordless
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FeedbackPage : ContentPage
    {
        #region Constructors

        public FeedbackPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public FeedbackPage(string feedback) : this()
        {
            Feedback = feedback;
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

        public ImageSource SplashIcon => ImageSource.FromResource("Hyprsoft.Auth.Passwordless.icon.png", typeof(SplashPage).GetTypeInfo().Assembly);

        public string SplashVersion => $"Version: {DependencyService.Get<IAppInformation>().Version}";

        public Command RequestInviteCommand => new Command(() => Device.OpenUri(SharedSettings.AppWebUri));

        public bool RequestInviteIsVisible => String.IsNullOrWhiteSpace(SecureStorage.GetAsync(App.SettingsKeyBearerToken).Result);

        #endregion
    }
}