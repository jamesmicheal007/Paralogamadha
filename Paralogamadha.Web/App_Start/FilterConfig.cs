

// ============================================================
//  Paralogamadha.Web / App_Start / FilterConfig.cs
// ============================================================

using System.Web.Mvc;

namespace Paralogamadha.Web.App_Start
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new RequireHttpsAttribute());
        }
    }
}