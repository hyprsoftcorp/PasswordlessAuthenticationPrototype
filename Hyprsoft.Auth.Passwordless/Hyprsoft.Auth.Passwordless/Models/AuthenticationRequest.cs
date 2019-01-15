using System.ComponentModel.DataAnnotations;

namespace Hyprsoft.Auth.Passwordless.Models
{
    /// <summary>
    /// Represents an authentiction request containing a unique user identifier and a timed-one-time-password (TOTP) token.
    /// </summary>
    public class AuthenticationRequest
    {
        #region Properties

        /// <summary>
        /// Unique user identifier.
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// Timed-one-time-password (TOTP) token.
        /// </summary>
        [Required]
        public string Token { get; set; }

        #endregion
    }
}
