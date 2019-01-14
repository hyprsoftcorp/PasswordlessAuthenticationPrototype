using Xamarin.Forms.Xaml;

namespace Hyprsoft.Auth.Passwordless
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class InviteNeededPage : BaseContentPage
	{
        #region Constructors

        public InviteNeededPage ()
		{
			InitializeComponent ();
            Title = "Invitation Needed";
		}

        #endregion
    }
}