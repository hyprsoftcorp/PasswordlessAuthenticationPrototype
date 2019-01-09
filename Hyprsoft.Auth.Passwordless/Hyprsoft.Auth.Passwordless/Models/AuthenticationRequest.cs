using System.ComponentModel.DataAnnotations;

namespace Hyprsoft.Auth.Passwordless.Models
{
    public class AuthenticationRequest
    {
        #region Properties

        [Required]
        public string Id { get; set; }

        [Required]
        public string Token { get; set; }

        #endregion
    }
}
