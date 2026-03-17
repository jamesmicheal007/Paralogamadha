// ============================================================
//  Paralogamadha.Web / Controllers / ErrorController.cs
//  Handles custom error pages — intentionally does NOT inherit
//  BaseController to avoid DB failures cascading into errors.
// ============================================================

using System.Net;
using System.Web.Mvc;

namespace Paralogamadha.Web.Controllers
{
    public class ErrorController : Controller
    {
        // ~/error  (defaultRedirect fallback)
        public ActionResult Index()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return View("Server");
        }

        // ~/error/notfound  (404)
        public ActionResult NotFound()
        {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return View();
        }

        // ~/error/server  (500)
        public ActionResult Server()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return View();
        }
    }
}
