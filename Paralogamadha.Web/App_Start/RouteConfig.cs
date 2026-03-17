// ============================================================
//  Paralogamadha.Web / App_Start / RouteConfig.cs
// ============================================================

using System.Web.Mvc;
using System.Web.Routing;

namespace Paralogamadha.Web.App_Start
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Localized routes — lang prefix: /ta/home, /hi/donation, etc.
            routes.MapRoute(
                name: "Localized",
                url: "{lang}/{controller}/{action}/{id}",
                defaults: new { action = "Index", id = UrlParameter.Optional },
                constraints: new { lang = @"en|ta|hi|fr" }
            );

            // SEO utility routes
            routes.MapRoute("Sitemap", "sitemap.xml", new { controller = "Seo", action = "Sitemap" });
            routes.MapRoute("Robots",  "robots.txt",  new { controller = "Seo", action = "Robots" });

            // Default route (English)
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

