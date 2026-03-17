// ============================================================
//  Paralogamadha.Web / Global.asax.cs
// ============================================================

using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Paralogamadha.Web.App_Start;

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

        protected void Application_Error()
        {
            var ex = Server.GetLastError();
            // TODO: Log to Serilog/NLog
            System.Diagnostics.Trace.TraceError($"Unhandled error: {ex}");
        }
    }
}
