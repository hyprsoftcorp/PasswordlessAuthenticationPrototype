using System;

namespace Hyprsoft.Auth.Passwordless
{
    public static class SharedSettings
    {
        public const string AppName = "Pwdless Auth Prototype";

        public static readonly Uri AppWebUri = new Uri("https://pwdlessauth.azurewebsites.net/");

        public static readonly Uri AppStoreGoogleUri = new Uri("https://play.google.com/store/apps/details?id=com.hyprsoft.Auth.Passwordless.Prototype");

        public static readonly Uri AppStoreAppleUri = new Uri("https://www.apple.com/ios/app-store");
    }
}
