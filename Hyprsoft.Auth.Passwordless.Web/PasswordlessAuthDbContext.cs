using Hyprsoft.Auth.Passwordless.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hyprsoft.Auth.Passwordless.Web
{
    public class PasswordlessAuthDbContext : IdentityDbContext
    {
        #region Constructors

        public PasswordlessAuthDbContext(DbContextOptions<PasswordlessAuthDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        #endregion

        #region Properties

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        #endregion

        #region Methods

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Fluent API not avaialble via Data Annotations.
            builder.Entity<PasswordlessAuthIdentityUser>().Property(p => p.CreatedUtc).HasDefaultValueSql("date('now')");
        }

        #endregion
    }
}
