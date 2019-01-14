using Xamarin.Forms.Xaml;

namespace Hyprsoft.Auth.Passwordless
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AccessDeniedPage : BaseContentPage
	{
        #region Constructors

        public AccessDeniedPage ()
		{
			InitializeComponent ();
            Title = "Access Denied";
        }

        #endregion
    }
}