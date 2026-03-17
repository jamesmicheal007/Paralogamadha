namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    using Paralogamadha.Core.Interfaces;
    using System.Web.Mvc;

    public class PrayerRequestAdminController : AdminBaseController
    {
        private readonly IEmailService _email;

        public PrayerRequestAdminController(IUnitOfWork uow, IFileUploadService upload, IEmailService email)
            : base(uow, upload)
        {
            _email = email;
        }

        // 1. Explicit path for Index (list of requests)
        public ActionResult Index(byte? status = null)
        {
            var model = _uow.PrayerRequests.GetAll(status);
            return View("~/Areas/Admin/Views/PrayerRequests/Index.cshtml", model);
        }

        // 2. Explicit path for Detail (viewing a single request)
        public ActionResult Detail(int id)
        {
            var model = _uow.PrayerRequests.GetById(id);
            if (model == null) return HttpNotFound();

            return View("~/Areas/Admin/Views/PrayerRequests/Detail.cshtml", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult Review(int id, byte statusId, string adminNotes)
        {
            try
            {
                _uow.PrayerRequests.Review(id, statusId, adminNotes, CurrentUserId);

                //// Example: Sending email notification if status is 'Prayed For' (Status 2)
                //if (statusId == 2)
                //{
                //    var request = _uow.PrayerRequests.GetById(id);
                //    if (request != null && !string.IsNullOrEmpty(request.Email))
                //    {
                //        _email.SendEmail(request.Email,
                //            "Prayer Request Update",
                //            "The community at Paralogamadha Basilica has received your request and is praying for your intentions.");
                //    }
                //}

                LogAudit("REVIEW", "prayerRequests", id, "Status -> " + statusId);
                return JsonOk(message: "Prayer request updated.");
            }
            catch (System.Exception ex)
            {
                return JsonFail("Failed to update: " + ex.Message);
            }
        }
    }
}