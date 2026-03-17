
// ============================================================
//  Paralogamadha.Web / App_Start / BundleConfig.cs
// ============================================================

using System.Web.Optimization;

namespace Paralogamadha.Web.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // ── Public CSS ────────────────────────────────────────
            bundles.Add(new StyleBundle("~/bundles/css/site").Include(
                "~/Content/css/site.css",
                "~/Content/css/animations.css"
            ));

            bundles.Add(new StyleBundle("~/bundles/css/admin").Include(
                "~/Content/css/admin.css"
            ));

            // ── Public JS ─────────────────────────────────────────
            bundles.Add(new ScriptBundle("~/bundles/js/site").Include(
                "~/Content/js/app.js"
            ));

            // ── Admin JS ──────────────────────────────────────────
            bundles.Add(new ScriptBundle("~/bundles/js/admin").Include(
                "~/Content/js/admin.js"
            ));

            BundleTable.EnableOptimizations = true;
        }
    }
}