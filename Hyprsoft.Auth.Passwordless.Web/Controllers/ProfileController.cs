using Hyprsoft.Auth.Passwordless.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace Hyprsoft.Auth.Passwordless.Web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController, Route("api/[controller]/[action]"), Produces("application/json")]
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

        /// <summary>
        /// Get's the current user's profile information.
        /// </summary>
        /// <returns>The current user's profile.  <see cref="UserProfile"/>.</returns>
        /// <response code="401">Returned if the user is not found.</response>
        /// <response code="403">Returned if the user is disabled.</response>
        [ProducesResponseType(401), ProducesResponseType(403)]
        [HttpGet]
        public ActionResult<UserProfile> Me()
        {
            var user = _userManager.Users.First(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (user == null)
                return Unauthorized();

            if (!user.IsEnabled)
                return Forbid();

            return Ok(new UserProfile { Name = user.Name, Email = user.Email });
        }

        #endregion
    }
}
