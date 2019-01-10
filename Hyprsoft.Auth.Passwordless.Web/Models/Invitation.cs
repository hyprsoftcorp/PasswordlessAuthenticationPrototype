using System.ComponentModel.DataAnnotations;

namespace Hyprsoft.Auth.Passwordless.Web.Models
{
    public class InvitationRequest
    {
        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
