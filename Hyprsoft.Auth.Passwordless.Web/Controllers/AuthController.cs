using System;
using System.Threading.Tasks;
using Hyprsoft.Auth.Passwordless.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hyprsoft.Auth.Passwordless.Web.Controllers
{
    /// <summary>
    /// Provides authentication services for this platform.
    /// </summary>
    [ApiController, Route("api/[controller]/[action]"), Produces("application/json")]
    public class AuthController : ControllerBase
    {
        #region Fields

        private readonly AuthenticationService _authenticationService;

        #endregion

        #region Constructors

        public AuthController(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends an automated email invitation to the specified recipient.
        /// </summary>
        /// <param name="model">The <see cref="InvitationRequest"/> containing the recipient's information.</param>
        /// <returns>A <see cref="AuthenticationRequest"/> containing the user's id and timed-one-time-password token.</returns>
        /// <response code="403">Returned if the user is disabled.</response>
        /// <response code="500">Returned if there is an unexpected exception.</response>
        [ProducesResponseType(403), ProducesResponseType(500)]
        [HttpPost]
        public async Task<ActionResult<AuthenticationRequest>> Invite(InvitationRequest model)
        {
            try
            {
                return Ok(await _authenticationService.SendInviteAsync(model));
            }
            catch (AuthenticationServiceForbiddenException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected exception: {ex.Message}.");
            }
        }

        /// <summary>
        /// Gets an access token and refresh token to be used in subsequent API requests.
        /// </summary>
        /// <param name="model">The <see cref="AuthenticationRequest"/> containing the user's id and timed-one-time-password token.</param>
        /// <returns>A <see cref="AuthenticationResponse"/> containing the user's new access and refresh tokens.</returns>
        /// <response code="401">Returned if the user is not found.</response>
        /// <response code="403">Returned if the user is disabled or the timed-one-time-password token is invalid or expired.</response>
        /// <response code="500">Returned if there is an unexpected exception.</response>
        [ProducesResponseType(401), ProducesResponseType(403), ProducesResponseType(500)]
        [HttpPost]
        public async Task<ActionResult<AuthenticationResponse>> Token(AuthenticationRequest model)
        {
            try
            {
                return Ok(await _authenticationService.AuthenticateAsync(model));
            }
            catch (AuthenticationServiceForbiddenException)
            {
                return Forbid();
            }
            catch (AuthenticationServiceUnauthorizedException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected exception: {ex.Message}.");
            }
        }

        /// <summary>
        /// Refreshes the user's access token using the specifed refresh token.
        /// </summary>
        /// <param name="model">The <see cref="AuthenticationResponse"/> containing the user's access and refresh tokens.</param>
        /// <returns>A <see cref="AuthenticationResponse"/> containing the user's new access and refresh tokens.</returns>
        /// <response code="401">Returned if the user is not found.</response>
        /// <response code="403">Returned if the user is disabled or the refresh token is invalid or expired.</response>
        /// <response code="500">Returned if there is an unexpected exception.</response>
        [ProducesResponseType(401), ProducesResponseType(403), ProducesResponseType(500)]
        [HttpPost]
        public async Task<ActionResult<AuthenticationResponse>> Refresh(AuthenticationResponse model)
        {
            try
            {
                return Ok(await _authenticationService.RefreshAuthenticationAsync(model));
            }
            catch (AuthenticationServiceForbiddenException)
            {
                return Forbid();
            }
            catch (AuthenticationServiceUnauthorizedException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected exception: {ex.Message}.");
            }
        }

        #endregion
    }
}
