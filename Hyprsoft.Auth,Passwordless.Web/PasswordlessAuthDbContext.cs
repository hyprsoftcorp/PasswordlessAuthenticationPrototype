using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hyprsoft.Auth.Passwordless.Web
{
    public class PasswordlessAuthDbContext : IdentityDbContext
    {
        public PasswordlessAuthDbContext(DbContextOptions<PasswordlessAuthDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<PasswordlessAuthIdentityUser>().Property(p => p.CreatedUtc).HasDefaultValueSql("getutcdate()");
            builder.Entity<PasswordlessAuthIdentityUser>().Property(p => p.Name).IsRequired();
        }
    }
}
