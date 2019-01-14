using Hyprsoft.Auth.Passwordless.Models;
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
            Feedback = $"You're {user.Name} and you're authenticated using '{user.Email}'.  Thanks for trying out our password-less authentication app.";
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

        #endregion
    }
}