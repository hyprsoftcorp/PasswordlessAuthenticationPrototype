using System.ComponentModel.DataAnnotations;

namespace Hyprsoft.Auth.Passwordless.Web.Models
{
    public class EmailSettings
    {
        public bool IsEnabled { get; set; }

        [Required]
        public string Host { get; set; }

        [Required]
        public int Port { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required, EmailAddress]
        public string FromEmail { get; set; }
    }
}
