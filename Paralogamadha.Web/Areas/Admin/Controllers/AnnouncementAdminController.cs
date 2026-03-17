
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

        // Explicitly point to the folder 'Announcements' instead of the default 'AnnouncementAdmin'
        public ActionResult Index() => View("~/Areas/Admin/Views/Announcements/Index.cshtml", _uow.Announcements.GetAll());
        // Update these actions to point to the actual folder path
        public ActionResult Create() => View("~/Areas/Admin/Views/Announcements/Edit.cshtml", new Announcement());

        public ActionResult Edit(int id)
        {
            var model = _uow.Announcements.GetById(id);
            if (model == null) return HttpNotFound();

            return View("~/Areas/Admin/Views/Announcements/Edit.cshtml", model);
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public ActionResult Save(Announcement model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Areas/Admin/Views/Announcements/Edit.cshtml", model);
            }

            model.Body = SanitizeHtml(model.Body ?? "");
            model.CreatedBy = CurrentUserId.ToString();

            var id = _uow.Announcements.Upsert(model);
            LogAudit(model.AnnouncementId == 0 ? "CREATE" : "UPDATE", "announcements", id);

            TempData["Success"] = "Announcement saved.";

            // Explicitly redirect to the Index action within the Admin Area
            return RedirectToAction("Index", "AnnouncementAdmin", new { area = "Admin" });
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