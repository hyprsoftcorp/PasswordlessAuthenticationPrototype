using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Reflection;

namespace Hyprsoft.Auth.Passwordless.Web
{
    public static class Extensions
    {
        #region Methods

        public static string GetAssemblyInformationalVersion(this IHtmlHelper helper)
        {
            return (((AssemblyInformationalVersionAttribute)Assembly.GetExecutingAssembly().GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute))).InformationalVersion);
        }

        public static DateTime GetAssemblyBuildDateTime(this IHtmlHelper helper)
        {
            var attribute = Assembly.GetExecutingAssembly().GetCustomAttribute<BuildDateAttribute>();
            return attribute != null ? attribute.DateTimeUtc : default(DateTime);
        }

        #endregion
    }
}
