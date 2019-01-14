using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Hyprsoft.Auth.Passwordless.Models;
using Hyprsoft.Auth.Passwordless.Web.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MimeKit;

namespace Hyprsoft.Auth.Passwordless.Web
{
    public class AuthenticationService
    {
        #region Fields

        private readonly UserManager<PasswordlessAuthIdentityUser> _userManager;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<AuthenticationService> _logger;

        #endregion

        #region Constructors

        public AuthenticationService(AuthenticationServiceOptions options, UserManager<PasswordlessAuthIdentityUser> userManager, IConfiguration config, ILogger<AuthenticationService> logger)
        {
            Options = options;
            _userManager = userManager;
            _emailSettings = config.GetSection("Email").Get<EmailSettings>();
            _logger = logger;
        }

        #endregion

        #region Properties

        public AuthenticationServiceOptions Options { get; }

        #endregion

        #region Methods

        public async Task<AuthenticationRequest> SendInviteAsync(InvitationRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                user = new PasswordlessAuthIdentityUser { UserName = request.Email, Email = request.Email, Name = request.Name };
                var result = await _userManager.CreateAsync(user);
                _logger.LogInformation($"[{nameof(SendInviteAsync)}] Created user '{user.Name}' with an id of '{user.Id}'.  Success: '{result.Succeeded}'.");
            }

            if (!user.IsEnabled)
            {
                _logger.LogWarning($"[{nameof(SendInviteAsync)}] User '{user.Id}' is disabled.");
                throw new AuthenticationServiceForbiddenException();
            }

            var token = await _userManager.GenerateUserTokenAsync(user, Options.OtpProvider, Options.OtpPurpose);
            _logger.LogInformation($"[{nameof(SendInviteAsync)}] Generated one-time-password token '{token}' for user '{user.Id}'.");

            if (_emailSettings != null && _emailSettings.IsEnabled)
            {
                if (!Validator.TryValidateObject(_emailSettings, new ValidationContext(_emailSettings), null))
                    throw new InvalidOperationException("Email settings are invalid.  Unable to send email.");

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.SslOnConnect);
                    await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);

                    var magicLink = $"{SharedSettings.AppWebUri}api/auth/token?id={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(token)}";
                    var message = new MimeMessage
                    {
                        Subject = $"{SharedSettings.AppName} Invitation",
                        Body = new BodyBuilder
                        {
                            HtmlBody = $"<!DOCTYPE html>" +
                            $"<html>" +
                            $"<head>" +
                            $"<title>{SharedSettings.AppName} Invitation</title>" +
                            $"</head>" +
                            $"<body>" +
                            $"<table bgcolor=\"#f8f8f8\" border=\"0\" cellpadding=\"10\" cellspacing=\"0\" style=\"background-color: #f8f8f8; width: 100%;\"><tbody><tr><td width=\"120\"><img src=\"{SharedSettings.AppWebUri}images/logo.png\" width=\"100\" height=\"100\" /></td><td><h2>{SharedSettings.AppName} Invitation</h2></td></tr></tbody></table>" +
                            $"<table bgcolor=\"#fcfcfc\" border=\"0\" cellpadding=\"30\" cellspacing=\"0\" style=\"background-color: #fcfcfc; width: 100%;\"><tbody><tr><td>" +
                            $"<p>Congrats {request.Name}, you have been invited to try out our password-less authentication app.  Our hope is that all app makers do away with usernames and passwords forever!  We'd love to get your feedback on this app using <a href=\"mailto:support@hyprsoft.com?subject={SharedSettings.AppName} Feedback\">support@hyprsoft.com</a>.</p>" +
                            $"<h3>STEP 1</h3>" +
                            $"<p>Download the '{SharedSettings.AppName}' app from the appropriate app store.</p><p><a href=\"{SharedSettings.AppStoreGoogleUri}\">Google Play Store</a> | <a href=\"{SharedSettings.AppStoreAppleUri}\">Apple App Store</a></p>" +
                            $"<h3>STEP 2</h3>" +
                            $"<p>Once the app is installed, open the link below on your mobile device.  <b>NOTE: The link below expires in {Options.OtpTokenLifespan.TotalMinutes} minutes and can only be used once</b>.  If you uninstall the app you will need to <a href=\"{SharedSettings.AppWebUri}\">request another invite</a>.<br /><a href=\"{magicLink}\"><h3>OPEN THIS LINK</h3></a></p>" +
                            $"<p align=\"center\"><small>Generated: {DateTime.Now.ToString("f")} | Copyright © {DateTime.Now.Year} by <a href=\"https://www.hyprsoft.com/\">Hyprsoft Corporation</a></small></p>" +
                            $"</td></tr></tbody></table>" +
                            $"</body>" +
                            $"</html>"
                        }.ToMessageBody()
                    };
                    message.From.Add(new MailboxAddress(_emailSettings.FromEmail));
                    message.To.Add(new MailboxAddress(request.Email));
                    message.Bcc.Add(new MailboxAddress("support@hyprsoft.com"));
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }   // using smtp client
            }
            else
            {
                _logger.LogWarning($"[{nameof(SendInviteAsync)}] Email settings are invalid or email is disabled.");
            }   // email settings valid?

            return new AuthenticationRequest { Id = user.Id, Token = token };
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.Id);
            if (user == null)
            {
                _logger.LogWarning($"[{nameof(AuthenticateAsync)}] User '{request.Id}' not found.");
                throw new AuthenticationServiceUnauthorizedException();
            }

            if (!user.IsEnabled)
            {
                _logger.LogWarning($"[{nameof(AuthenticateAsync)}] User '{user.Id}' is disabled.");
                throw new AuthenticationServiceForbiddenException();
            }

            if (!await _userManager.VerifyUserTokenAsync(user, Options.OtpProvider, Options.OtpPurpose, request.Token))
            {
                _logger.LogWarning($"[{nameof(AuthenticateAsync)}] User '{user.Id}' one-time-password token '{request.Token}' verification failed.");
                throw new AuthenticationServiceForbiddenException();
            }

            var refreshToken = await AuthenticationResponse.GenerateRefreshTokenAsync();
            user.RefreshTokens.Add(new RefreshToken { ExpiresUtc = DateTime.UtcNow.Add(Options.BearerRefreshTokenLifespan), Token = refreshToken });
            var result = await _userManager.UpdateAsync(user);
            _logger.LogInformation($"[{nameof(AuthenticateAsync)}] Added refresh token '{refreshToken}' for user '{user.Id}'.  Success: '{result.Succeeded}'.");

            // This ensures the OTP cannot be used again.
            result = await _userManager.UpdateSecurityStampAsync(user);
            _logger.LogInformation($"[{nameof(AuthenticateAsync)}] Updated security stamp for user '{user.Id}'.  Success: '{result.Succeeded}'.");

            return new AuthenticationResponse
            {
                AccessToken = GetAccessTokenForUser(user),
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthenticationResponse> RefreshAuthenticationAsync(AuthenticationResponse request)
        {
            var principal = GetPrincipalFromAccessToken(request.AccessToken);
            if (principal == null)
            {
                _logger.LogWarning($"[{nameof(RefreshAuthenticationAsync)}] Unable to get principal from access token '{request.AccessToken}'.");
                throw new AuthenticationServiceUnauthorizedException();
            }

            var user = _userManager.Users.Include(u => u.RefreshTokens).SingleOrDefault(u => u.Id == principal.FindFirstValue(ClaimTypes.NameIdentifier));
            if (user == null)
            {
                _logger.LogWarning($"[{nameof(RefreshAuthenticationAsync)}] User '{user.Id}' not found.");
                throw new AuthenticationServiceUnauthorizedException();
            }

            var existingRefreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == request.RefreshToken);
            if (!user.IsEnabled || existingRefreshToken == null || (DateTime.UtcNow > existingRefreshToken.ExpiresUtc))
            {
                _logger.LogInformation($"[{nameof(RefreshAuthenticationAsync)}] Refresh token '{request.RefreshToken}' in invalid.  Reason: {(!user.IsEnabled ? "User is disabled" : existingRefreshToken == null ? "Token not found" : DateTime.UtcNow > existingRefreshToken.ExpiresUtc ? "Token Expired" : "Unknown")}.");
                throw new AuthenticationServiceForbiddenException();
            }

            var success = user.RefreshTokens.Remove(existingRefreshToken);
            _logger.LogInformation($"[{nameof(RefreshAuthenticationAsync)}] Removed refresh token '{existingRefreshToken.Token}' for user '{existingRefreshToken.UserId}'.  Success: '{success}'.");

            var refreshToken = await AuthenticationResponse.GenerateRefreshTokenAsync();
            user.RefreshTokens.Add(new RefreshToken { ExpiresUtc = DateTime.UtcNow.Add(Options.BearerRefreshTokenLifespan), Token = refreshToken });
            var result = await _userManager.UpdateAsync(user);
            _logger.LogInformation($"[{nameof(RefreshAuthenticationAsync)}] Added refresh token '{refreshToken}' for user '{existingRefreshToken.UserId}'.  Success: '{result.Succeeded}'.");

            return new AuthenticationResponse
            {
                AccessToken = GetAccessTokenForUser(user),
                RefreshToken = refreshToken
            };
        }

        private ClaimsPrincipal GetPrincipalFromAccessToken(string accessToken)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Options.BearerTokenIssuer,
                ValidAudience = Options.BearerTokenAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Options.BearerTokenSecurityKey))
            };

            ClaimsPrincipal principal = null;
            try
            {
                principal = new JwtSecurityTokenHandler().ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
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
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Options.BearerTokenSecurityKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwtToken = new JwtSecurityToken(
                issuer: Options.BearerTokenIssuer,
                audience: Options.BearerTokenAudience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.Add(Options.BearerAccessTokenLifespan),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }

        #endregion
    }
}
