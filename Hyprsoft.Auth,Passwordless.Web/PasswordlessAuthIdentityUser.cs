using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hyprsoft.Auth.Passwordless.Web
{
    public class PasswordlessAuthIdentityUser : IdentityUser
    {
        public DateTime CreatedUtc { get; set; }

        [Required]
        public string Name { get; set; }

        public bool IsEnabled { get; set; } = true;
    }
}
