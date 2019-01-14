using System;
using System.Threading.Tasks;
using Hyprsoft.Auth.Passwordless.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hyprsoft.Auth.Passwordless.Web.Controllers
{
    [ApiController, Route("api/[controller]/[action]")]
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

        [HttpPost]
        public async Task<ActionResult<AuthenticationRequest>> Invite(InvitationRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

        [HttpPost]
        public async Task<ActionResult<AuthenticationResponse>> Token(AuthenticationRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                return Ok(await _authenticationService.GetTokensAsync(model));
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

        [HttpPost]
        public async Task<ActionResult<AuthenticationResponse>> Refresh(AuthenticationResponse model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                return Ok(await _authenticationService.RefreshTokensAsync(model));
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
