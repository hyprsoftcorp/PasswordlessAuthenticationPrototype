using System.ComponentModel.DataAnnotations;

namespace Hyprsoft.Auth.Passwordless.Models
{
    /// <summary>
    /// Represents an invitation to use this platform.
    /// </summary>
    public class InvitationRequest
    {
        #region Properties

        /// <summary>
        /// The first and last name of the person requesting the invite.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The email address of the person requesting the invite.
        /// </summary>
        [Required, EmailAddress]
        public string Email { get; set; }

        #endregion
    }
}
