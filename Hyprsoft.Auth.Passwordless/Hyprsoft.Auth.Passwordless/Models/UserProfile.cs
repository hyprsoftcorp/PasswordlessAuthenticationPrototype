using System.ComponentModel.DataAnnotations;

namespace Hyprsoft.Auth.Passwordless.Models
{
    public class UserProfile
    {
        #region Properties

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }

        #endregion
    }
}
