
// ============================================================
//  AnnouncementAdminController.cs
// ============================================================

namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    using Paralogamadha.Core.Interfaces;
    using Paralogamadha.Core.Models;
    using System.Web.Mvc;

    public class AnnouncementAdminController : AdminBaseController
    {
        public AnnouncementAdminController(IUnitOfWork uow, IFileUploadService upload)
            : base(uow, upload) { }

        public ActionResult Index() => View(_uow.Announcements.GetAll());
        public ActionResult Create() => View(new Announcement());
        public ActionResult Edit(int id) => View(_uow.Announcements.GetById(id) ?? new Announcement());

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public ActionResult Save(Announcement model)
        {
            if (!ModelState.IsValid) return View("Edit", model);

            // Sanitize rich text body
            var sanitizer = new Ganss.Xss.HtmlSanitizer();
            model.Body = sanitizer.Sanitize(model.Body ?? "");
            model.CreatedBy = CurrentUserId.ToString();

            var id = _uow.Announcements.Upsert(model);
            LogAudit(model.AnnouncementId == 0 ? "CREATE" : "UPDATE", "announcements", id);
            TempData["Success"] = "Announcement saved.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            _uow.Announcements.Delete(id);
            LogAudit("DELETE", "announcements", id);
            return JsonOk(message: "Announcement deleted.");
        }
    }
}