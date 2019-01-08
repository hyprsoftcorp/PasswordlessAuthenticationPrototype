using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Hyprsoft.Auth.Passwordless.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Hyprsoft.Auth.Passwordless.Web.Controllers
{
    [ApiController, Route("api/[controller]/[action]")]
    public class AuthController : ControllerBase
    {
        #region Fields

        private readonly UserManager<PasswordlessAuthIdentityUser> _userManager;

        #endregion

        #region Constructors

        public AuthController(UserManager<PasswordlessAuthIdentityUser> userManager)
        {
            _userManager = userManager;
        }

        #endregion

        #region Methods

        [HttpPost]
        public async Task<ActionResult> Token(AuthenticationRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return Unauthorized();

            if (user.IsEnabled && (String.Compare(user.Email, AuthenticationSettings.AppleTesterEmail, true) == 0 ||
                await _userManager.VerifyUserTokenAsync(user, AuthenticationSettings.OtpProvider, AuthenticationSettings.OtpPurpose, model.Token)))
            {
                await _userManager.UpdateSecurityStampAsync(user);

                var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id) };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthenticationSettings.BearerSecurityKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var jwtToken = new JwtSecurityToken(
                    issuer: AuthenticationSettings.BearerIssuer,
                    audience: AuthenticationSettings.DefaultBearerAudience,
                    claims: claims,
                    expires: DateTime.Now.Add(AuthenticationSettings.BearerTokenLifespan),
                    signingCredentials: credentials);

                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(jwtToken) });
            }

            return Forbid();
        }

        #endregion
    }
}
