// ============================================================
//  DashboardController.cs
// ============================================================

namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    using Paralogamadha.Core.Interfaces;
    using System.Web.Mvc;

    public class DashboardController : AdminBaseController
    {
        public DashboardController(IUnitOfWork uow, IFileUploadService upload)
            : base(uow, upload) { }

        public ActionResult Index()
        {
            var model = _uow.Dashboard.GetStats();
            return View(model);
        }
    }
}