using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Hyprsoft.Auth.Passwordless
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SplashPage : ContentPage
    {
        #region Constructors

        public SplashPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        #endregion

        #region Properties

        public ImageSource SplashIcon => ImageSource.FromResource("Hyprsoft.Auth.Passwordless.icon.png", typeof(SplashPage).GetTypeInfo().Assembly);

        public string SplashVersion => $"Version: {DependencyService.Get<IAppInformation>().Version}";

        #endregion
    }
}