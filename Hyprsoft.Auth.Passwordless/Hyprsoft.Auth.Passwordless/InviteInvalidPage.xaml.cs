using Xamarin.Forms.Xaml;

namespace Hyprsoft.Auth.Passwordless
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class InviteInvalidPage : BaseContentPage
	{
        #region Constructors

        public InviteInvalidPage ()
		{
			InitializeComponent ();
            Title = "Invalid Invitation";
        }

        #endregion
    }
}