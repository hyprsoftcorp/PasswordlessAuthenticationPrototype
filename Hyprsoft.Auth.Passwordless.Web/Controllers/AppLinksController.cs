using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Reflection;
using System.Text;

namespace Hyprsoft.Auth.Passwordless.Web.Controllers
{
    /// <summary>
    /// Used to validate app links on Android devices and universal links on iOS devices.
    /// </summary>
    public class AppLinksController : Controller
    {
        #region Methods

        /// <summary>
        /// Gets the 'assetlinks.json' file used to validate app links for this domain on Android devices.
        /// </summary>
        /// <returns>The assetlinks.json file.</returns>
        [HttpGet("~/.well-known/assetlinks.json")]
        public ActionResult Android()
        {
            return GetEmbeddedResourceAsJson("Hyprsoft.Auth.Passwordless.Web.assetlinks.json");
        }

        /// <summary>
        /// Gets the 'apple-app-site-association' json file used to validate universal links for this domain on iOS devices.
        /// </summary>
        /// <returns>The apple-app-site-association json file.</returns>
        [HttpGet("~/apple-app-site-association")]
        public ActionResult AppleRoot()
        {
            return GetEmbeddedResourceAsJson("Hyprsoft.Auth.Passwordless.Web.apple-app-site-association.json");
        }

        /// <summary>
        /// Gets the 'apple-app-site-association' json file used to validate universal links for this domain on iOS devices.
        /// </summary>
        /// <returns>The apple-app-site-association json file.</returns>
        [HttpGet("~/.well-known/apple-app-site-association")]
        public ActionResult AppleWellKnown()
        {
            return GetEmbeddedResourceAsJson("Hyprsoft.Auth.Passwordless.Web.apple-app-site-association.json");
        }

        private ContentResult GetEmbeddedResourceAsJson(string resourceKey)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceKey))
            {
                var bytes = new byte[stream.Length];
                using (var reader = new StreamReader(stream))
                    stream.Read(bytes, 0, bytes.Length);

                return Content(Encoding.UTF8.GetString(bytes, 0, bytes.Length), "application/json");
            }   // using manifest resource stream
        }

        #endregion
    }
}