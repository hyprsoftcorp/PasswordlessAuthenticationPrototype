using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyprsoft.Auth.Passwordless.Web.Models
{
    [Table("AspNetRefreshTokens")]
    public class RefreshToken
    {
        #region Properties

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        public string Token { get; set; }

        public DateTime ExpiresUtc { get; set; }

        [Required, ForeignKey("User")]
        public string UserId { get; set; }

        public PasswordlessAuthIdentityUser User { get; set; }

        #endregion
    }
}
