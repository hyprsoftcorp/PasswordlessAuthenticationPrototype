using System.ComponentModel.DataAnnotations;

namespace Hyprsoft.Auth.Passwordless.Models
{
    /// <summary>
    /// Represents a user's profile.
    /// </summary>
    public class UserProfile
    {
        #region Properties

        /// <summary>
        /// User's email address.
        /// </summary>
        [Required, EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// User's first and last name.
        /// </summary>
        [Required]
        public string Name { get; set; }

        #endregion
    }
}
