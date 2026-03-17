
// ============================================================
//  PrayerRequestAdminController.cs
// ============================================================

namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    using Paralogamadha.Core.Interfaces;
    using System.Web.Mvc;

    public class PrayerRequestAdminController : AdminBaseController
    {
        private readonly IEmailService _email;

        public PrayerRequestAdminController(IUnitOfWork uow, IFileUploadService upload, IEmailService email)
            : base(uow, upload) => _email = email;

        public ActionResult Index(byte? status = null) =>
            View(_uow.PrayerRequests.GetAll(status));

        public ActionResult Detail(int id) => View(_uow.PrayerRequests.GetById(id));

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult Review(int id, byte statusId, string adminNotes)
        {
            _uow.PrayerRequests.Review(id, statusId, adminNotes, CurrentUserId);
            LogAudit("REVIEW", "prayerRequests", id, $"Status → {statusId}");
            return JsonOk(message: "Prayer request updated.");
        }
    }
}