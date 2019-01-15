using Hyprsoft.Auth.Passwordless.Web.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyprsoft.Auth.Passwordless.Web
{
    public class PasswordlessAuthIdentityUser : IdentityUser
    {
        #region Properties

        public DateTime CreatedUtc { get; set; }

        [Required]
        public string Name { get; set; }

        [DefaultValue(true)]
        public bool IsEnabled { get; set; } = true;

        [ForeignKey("UserId")]
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        #endregion
    }
}
