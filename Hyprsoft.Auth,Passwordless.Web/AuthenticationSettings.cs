using System;

namespace Hyprsoft.Auth.Passwordless.Web
{
    public static class AuthenticationSettings
    {
        public static readonly string OtpProvider = "Default";

        public static readonly string OtpPurpose = "passwordless-auth";

        public static readonly TimeSpan OtpTokenLifespan = TimeSpan.FromMinutes(15);

        public static readonly string BearerSecurityKey = "A8B8DB6D-4DC7-4BA8-AFF9-F0A7567035EA-1954658E-D490-4CEC-9C15-C00851040B53-BE88D1E4-2249-408E-8902-15AB178937B9";

        public static readonly string BearerIssuer = "hyprsoft.com";

        public static readonly string DefaultBearerAudience = BearerIssuer;

        public static readonly TimeSpan BearerTokenLifespan = TimeSpan.FromDays(365);

        public static readonly string AppleTesterEmail = "appletester@hyprsoft.com";
    }
}
