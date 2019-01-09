using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Hyprsoft.Auth.Passwordless.Models;
using Hyprsoft.Auth.Passwordless.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<ActionResult<AuthenticationData>> Token(AuthenticationRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                    return Unauthorized();

                if (!user.IsEnabled || !await _userManager.VerifyUserTokenAsync(user, AuthenticationSettings.OtpProvider, AuthenticationSettings.OtpPurpose, model.Token))
                    return Forbid();

                var refreshToken = await AuthenticationData.GenerateRefreshTokenAsync();
                user.RefreshTokens.Add(new RefreshToken { ExpiresUtc = DateTime.UtcNow.Add(AuthenticationSettings.BearerRefreshTokenLifespan), Token = refreshToken });
                await _userManager.UpdateAsync(user);

                // This ensures the OTP cannot be used again.
                await _userManager.UpdateSecurityStampAsync(user);

                return Ok(new AuthenticationData
                {
                    AccessToken = GetAccessTokenForUser(user),
                    RefreshToken = refreshToken
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<AuthenticationData>> Refresh(AuthenticationData model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var principal = GetPrincipalFromAccessToken(model.AccessToken);
                if (principal == null)
                    return Unauthorized();

                var user = _userManager.Users.Include(u => u.RefreshTokens).SingleOrDefault(u => u.Id == principal.FindFirstValue(ClaimTypes.NameIdentifier));
                if (user == null)
                    return Unauthorized();

                var existingRefreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == model.RefreshToken);
                if (!user.IsEnabled || existingRefreshToken == null || (DateTime.UtcNow > existingRefreshToken.ExpiresUtc))
                    return Forbid();

                user.RefreshTokens.Remove(existingRefreshToken);

                var refreshToken = await AuthenticationData.GenerateRefreshTokenAsync();
                user.RefreshTokens.Add(new RefreshToken { ExpiresUtc = DateTime.UtcNow.Add(AuthenticationSettings.BearerRefreshTokenLifespan), Token = refreshToken });
                await _userManager.UpdateAsync(user);

                return Ok(new AuthenticationData
                {
                    AccessToken = GetAccessTokenForUser(user),
                    RefreshToken = refreshToken
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private ClaimsPrincipal GetPrincipalFromAccessToken(string accessToken)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = AuthenticationSettings.BearerTokenIssuer,
                ValidAudience = AuthenticationSettings.BearerTokenAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthenticationSettings.BearerTokenSecurityKey))
            };

            ClaimsPrincipal principal = null;
            try
            {
                principal = new JwtSecurityTokenHandler().ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    return null;
            }
            catch (Exception)
            {
                // If for any reason we can't validate our access token let's just return null.
            }

            return principal;
        }

        private string GetAccessTokenForUser(PasswordlessAuthIdentityUser user)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id) };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthenticationSettings.BearerTokenSecurityKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwtToken = new JwtSecurityToken(
                issuer: AuthenticationSettings.BearerTokenIssuer,
                audience: AuthenticationSettings.BearerTokenAudience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.Add(AuthenticationSettings.BearerAccessTokenLifespan),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }

        #endregion
    }
}
