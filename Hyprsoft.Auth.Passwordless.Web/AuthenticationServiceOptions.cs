using System;

namespace Hyprsoft.Auth.Passwordless.Web
{
    public class AuthenticationServiceOptions
    {
        #region Properties

        public string OtpProvider { get; set; } = "Default";

        public string OtpPurpose { get; set; } = "passwordless-auth";

        public TimeSpan OtpTokenLifespan { get; set; } = TimeSpan.FromMinutes(15);

        public string BearerTokenSecurityKey { get; set; }

        public string BearerTokenIssuer { get; set; } = "http://www.hyprsoft.com/pwdlessauth";

        public string BearerTokenAudience { get; set; } = "passwordlessauth";

        public TimeSpan BearerTokenClockSkew { get; set; } = TimeSpan.Zero;

        public TimeSpan BearerAccessTokenLifespan { get; set; } = TimeSpan.FromHours(8);

        public TimeSpan BearerRefreshTokenLifespan { get; set; } = TimeSpan.FromDays(90);

        #endregion
    }
}
