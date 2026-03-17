// ============================================================
//  Paralogamadha.Web / Global.asax.cs
// ============================================================

using Paralogamadha.Web.App_Start;
using System;
using System.Globalization;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace Paralogamadha.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            UnityConfig.RegisterComponents();

            // Disable X-Powered-By header
            MvcHandler.DisableMvcResponseHeader = true;
        }

        protected void Application_BeginRequest()
        {
            // Remove Server header
            Response.Headers.Remove("Server");
            Response.Headers.Remove("X-AspNet-Version");
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            try
            {
                var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(Context));
                // Resolve culture from route or cookie
                string lang = routeData?.Values["lang"]?.ToString() ?? "en";

                var supportedCultures = new[] { "en", "ta", "hi" };
                if (!Array.Exists(supportedCultures, l => l == lang))
                    lang = "en";

                var culture = new CultureInfo(lang == "ta" ? "ta-IN" : lang == "hi" ? "hi-IN" : "en-US");
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;

                // Store for use in controllers / views
                HttpContext.Current.Items["CurrentLang"] = lang;
            }
            catch
            {
                // Fallback to default if routing/state fails during an error
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            }
        }
        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                try
                {
                    var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                    if (authTicket != null && !authTicket.Expired)
                    {
                        // userData looks like "123|SuperAdmin"
                        var data = authTicket.UserData.Split('|');
                        if (data.Length >= 2)
                        {
                            string userId = data[0];
                            string[] roles = { data[1] }; // The role we stored

                            // Create the Identity and Principal
                            var identity = new GenericIdentity(authTicket.Name);
                            var principal = new GenericPrincipal(identity, roles);

                            // This is what the [Authorize] attribute actually checks!
                            HttpContext.Current.User = principal;
                        }
                    }
                }
                catch
                {
                    // Decryption failed or ticket tampered with
                }
            }
        }
        protected void Application_Error()
        {
            var ex = Server.GetLastError();
            // TODO: Log to Serilog/NLog
            System.Diagnostics.Trace.TraceError($"Unhandled error: {ex}");
        }
    }
}
