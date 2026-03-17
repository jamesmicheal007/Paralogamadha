// ============================================================
//  Paralogamadha.Web / Areas / Admin / AdminAreaRegistration.cs
// ============================================================

using System.Web.Mvc;

namespace Paralogamadha.Web.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName => "Admin";

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name:     "Admin_default",
                url:      "admin/{controller}/{action}/{id}",
                defaults: new { controller = "Dashboard", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
