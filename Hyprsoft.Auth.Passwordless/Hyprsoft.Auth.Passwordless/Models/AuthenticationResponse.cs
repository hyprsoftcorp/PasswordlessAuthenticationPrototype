using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Hyprsoft.Auth.Passwordless.Models
{
    public class AuthenticationResponse
    {
        #region Properties

        [Required]
        public string AccessToken { get; set; }

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
