using System.Web.Mvc;

namespace Paralogamadha.Web.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName => "Admin";

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Admin_default",
                url: "Admin/{controller}/{action}/{id}",
                defaults: new { controller = "Dashboard", action = "Index", id = UrlParameter.Optional },
                // Add this to prevent route collisions with the main site
                namespaces: new[] { "Paralogamadha.Web.Areas.Admin.Controllers" }
            );
        }
    }
}