using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Reflection;
using System.Text;

namespace Hyprsoft.Auth.Passwordless.Web.Controllers
{
    public class AppLinksController : Controller
    {
        #region Methods

        [Route("~/.well-known/assetlinks.json")]
        public ActionResult Android()
        {
            return GetEmbeddedResourceAsJson("Hyprsoft.Auth.Passwordless.Web.assetlinks.json");
        }

        [Route("~/apple-app-site-association")]
        public ActionResult AppleRoot()
        {
            return GetEmbeddedResourceAsJson("Hyprsoft.Auth.Passwordless.Web.apple-app-site-association.json");
        }

        [Route("~/.well-known/apple-app-site-association")]
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