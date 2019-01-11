using Hyprsoft.Auth.Passwordless.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Hyprsoft.Auth.Passwordless.Web.Controllers
{
    public class HomeController : Controller
    {
        #region Fields

        private readonly AuthenticationService _authenticationService;

        #endregion

        #region Constructors

        public HomeController(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        #endregion

        #region Methods

        public ActionResult Index()
        {
            return View(new InvitationRequest());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(InvitationRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _authenticationService.SendInviteAsync(model);
                ViewBag.Feedback = $"Your invitation was successfully sent to '{model.Email}'.  The invitation contains instructions on how to download our app.";
                ModelState.Clear();
                model = new InvitationRequest();
            }
            catch (AuthenticationServiceForbiddenException)
            {
                ViewBag.Feedback = $"Unfortunately invitations for this user can no longer be requested.";
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Uh oh.  Something went wrong that we didn't expect.  Please try again later.  Details: {ex.Message}.";
            }

            return View(model);
        }

        #endregion
    }
}
