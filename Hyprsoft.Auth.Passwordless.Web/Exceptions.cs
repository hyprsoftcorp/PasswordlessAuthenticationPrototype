using System;

namespace Hyprsoft.Auth.Passwordless.Web
{
    public class AuthenticationServiceUnauthorizedException : Exception
    {
        #region Constructors

        public AuthenticationServiceUnauthorizedException() : base() { }

        public AuthenticationServiceUnauthorizedException(string message) : base(message) { }

        public AuthenticationServiceUnauthorizedException(string message, Exception innerException) : base(message, innerException) { }

        #endregion
    }

    public class AuthenticationServiceForbiddenException : Exception
    {
        #region Constructors

        public AuthenticationServiceForbiddenException() : base() { }

        public AuthenticationServiceForbiddenException(string message) : base(message) { }

        public AuthenticationServiceForbiddenException(string message, Exception innerException) : base(message, innerException) { }

        #endregion
    }
}
