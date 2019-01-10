using System;

namespace Hyprsoft.Auth.Passwordless.Web
{
    public class AuthenticationServiceOptions
    {
        #region Properties

        public string OtpProvider { get; set; } = "Default";

        public string OtpPurpose { get; set; } = "passwordless-auth";

        public TimeSpan OtpTokenLifespan { get; set; } = TimeSpan.FromMinutes(15);

        public string BearerTokenSecurityKey { get; set; } = "A8B8DB6D-4DC7-4BA8-AFF9-F0A7567035EA-1954658E-D490-4CEC-9C15-C00851040B53-BE88D1E4-2249-408E-8902-15AB178937B9";

        public string BearerTokenIssuer { get; set; } = "hyprsoft.com";

        public string BearerTokenAudience { get; set; } = "hyprsoft.com";

        public TimeSpan BearerTokenClockSkew { get; set; } = TimeSpan.Zero;

        public TimeSpan BearerAccessTokenLifespan { get; set; } = TimeSpan.FromDays(1);

        public TimeSpan BearerRefreshTokenLifespan { get; set; } = TimeSpan.FromDays(365);

        #endregion
    }
}
