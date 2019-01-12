using System;

namespace Hyprsoft.Auth.Passwordless
{
    public static class SharedSettings
    {
        public const string AppName = "Pwdless Authentication";

        public static readonly Uri AppWebUri = new Uri("https://pwdlessauth.azurewebsites.net/");

        public static readonly Uri AppStoreGoogleUri = new Uri("https://play.google.com/store/apps/details?id=com.hyprsoft.Auth.Passwordless.Prototype");

        public static readonly Uri AppStoreAppleUri = new Uri("https://testflight.apple.com/join/WuVy2oV7");
    }
}
