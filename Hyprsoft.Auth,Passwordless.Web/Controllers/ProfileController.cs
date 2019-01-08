using Hyprsoft.Auth.Passwordless.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Hyprsoft.Auth.Passwordless.Web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController, Route("api/[controller]/[action]")]
    public class ProfileController : ControllerBase
    {
        #region Fields

        private readonly UserManager<PasswordlessAuthIdentityUser> _userManager;

        #endregion

        #region Constructors

        public ProfileController(UserManager<PasswordlessAuthIdentityUser> userManager)
        {
            _userManager = userManager;
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<User> Me()
        {
            var user = _userManager.Users.First(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (user == null)
                return Unauthorized();

            if (!user.IsEnabled)
                return Forbid();

            return Ok(new User { Name = user.Name, Email = user.Email });
        }

        #endregion
    }
}
