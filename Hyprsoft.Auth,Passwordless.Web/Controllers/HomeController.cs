using Hyprsoft.Auth.Passwordless.Models;
using Hyprsoft.Auth.Passwordless.Web.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Hyprsoft.Auth.Passwordless.Web.Controllers
{
    public class HomeController : Controller
    {
        #region Fields

        private readonly UserManager<PasswordlessAuthIdentityUser> _userManager;
        private readonly EmailSettings _emailSettings;

        #endregion

        #region Constructors

        public HomeController(UserManager<PasswordlessAuthIdentityUser> userManager, EmailSettings emailSettings)
        {
            _userManager = userManager;
            _emailSettings = emailSettings;
        }

        #endregion

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
                if (!Validator.TryValidateObject(_emailSettings, new ValidationContext(_emailSettings), null))
                    throw new InvalidOperationException("Email settings are missing or invalid.");

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    user = new PasswordlessAuthIdentityUser { UserName = model.Email, Email = model.Email, Name = model.Name };
                    await _userManager.CreateAsync(user);
                }

                if (user.IsEnabled)
                {
                    var token = await _userManager.GenerateUserTokenAsync(user, AuthenticationSettings.OtpProvider, AuthenticationSettings.OtpPurpose);
                    var url = Url.Action(nameof(AuthController.Token), "Auth", new { id = user.Id, token }, Request.Scheme);

                    System.Diagnostics.Debug.WriteLine(url);
                    System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(new AuthenticationRequest { Id = user.Id, Token = token }));

                    if (_emailSettings.IsEnabled)
                    {
                        using (var client = new SmtpClient())
                        {
                            await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.SslOnConnect);
                            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                            var message = new MimeMessage
                            {
                                Subject = $"{SharedSettings.AppName} Invitation",
                                Body = new BodyBuilder { HtmlBody = $"<!DOCTYPE html><html><head><title>{SharedSettings.AppName} Invitation</title></head><body><h2>{SharedSettings.AppName} Invitation</h2><p>Congrats {model.Name}, you have been invited to try out our password-less authentication app.  Our hope is that all app vendors get rid of usernames and passwords forever!  We'd love to hear your thoughts on our app using <a href=\"mailto:support@hyprsoft.com?subject={SharedSettings.AppName} Feedback\">support@hyprsoft.com</a>.</p><h3>STEP 1</h3><p>Download the '{SharedSettings.AppName}' app from the appropriate app store.</p><p><a href=\"{SharedSettings.AppStoreGoogleUri}\">Google Play Store</a> | <a href=\"{SharedSettings.AppStoreAppleUri}\">Apple App Store</a></p><h3>STEP 2</h3><p><b>Once the app is installed</b>, open <a href=\"{url}\">this link</a> on our your mobile device.  <b>The link expires in {AuthenticationSettings.OtpTokenLifespan.TotalMinutes} minutes and can only be used once</b>.  If you uninstall the app or lose your phone you will need to <a href=\"{SharedSettings.AppWebUri}\">request another invite</a>.</p></body></html>" }.ToMessageBody()
                            };
                            message.From.Add(new MailboxAddress(_emailSettings.FromEmail));
                            message.To.Add(new MailboxAddress(model.Email));
                            message.Bcc.Add(new MailboxAddress("support@hyprsoft.com"));
                            await client.SendAsync(message);
                            await client.DisconnectAsync(true);
                        }   // using smtp client
                    }   // email enabled?

                    ViewBag.Feedback = $"Your invitation was successfully sent to '{model.Email}'.  Yor invitation will contain instructions on how to download the app.";
                    model = new InvitationRequest();
                }
                else
                    ViewBag.Feedback = $"But...invitations for this user can no longer be requested.";
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Uh oh.  Something went wrong that we didn't expect.  Please try again later.  Details: {ex.Message}.";
            }

            return View(model);
        }
    }
}
