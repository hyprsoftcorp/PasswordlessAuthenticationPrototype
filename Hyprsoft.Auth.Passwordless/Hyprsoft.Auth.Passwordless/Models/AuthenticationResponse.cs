using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Hyprsoft.Auth.Passwordless.Models
{
    /// <summary>
    /// Represents an authentiction response containing a bearer access token and refresh token.
    /// </summary>
    public class AuthenticationResponse
    {
        #region Properties

        /// <summary>
        /// Bearer access token.
        /// </summary>
        [Required]
        public string AccessToken { get; set; }

        /// <summary>
        /// Refresh token.
        /// </summary>
        [Required]
        public string RefreshToken { get; set; }

        #endregion

        #region Methods

        public static Task<string> GenerateRefreshTokenAsync(int size = 32)
        {
            var data = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(data);
                return Task.FromResult(Convert.ToBase64String(data));
            }
        }

        #endregion
    }
}
