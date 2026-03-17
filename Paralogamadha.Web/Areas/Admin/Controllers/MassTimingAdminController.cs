namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    using Paralogamadha.Core.Interfaces;
    using Paralogamadha.Core.Models;
    using System.Web.Mvc;

    public class MassTimingAdminController : AdminBaseController
    {
        public MassTimingAdminController(IUnitOfWork uow, IFileUploadService upload)
            : base(uow, upload) { }

        // 1. Explicit path for Index
        public ActionResult Index() =>
            View("~/Areas/Admin/Views/MassTiming/Index.cshtml", _uow.MassSchedules.GetAll());

        // 2. Explicit path for Create (pointing to Edit.cshtml as a shared form)
        public ActionResult Create() =>
            View("~/Areas/Admin/Views/MassTiming/Edit.cshtml", new MassSchedule());

        // 3. Explicit path for Edit
        public ActionResult Edit(int id)
        {
            var schedule = _uow.MassSchedules.GetById(id);
            if (schedule == null) return HttpNotFound();

            return View("~/Areas/Admin/Views/MassTiming/Edit.cshtml", schedule);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Save(MassSchedule model)
        {
            if (!ModelState.IsValid)
            {
                // Must specify path here if validation fails
                return View("~/Areas/Admin/Views/MassTiming/Edit.cshtml", model);
            }

            model.CreatedBy = CurrentUserId.ToString();
            var id = _uow.MassSchedules.Upsert(model);

            LogAudit(model.ScheduleId == 0 ? "CREATE" : "UPDATE", "massTiming", id);
            TempData["Success"] = "Mass schedule saved.";

            // 4. Explicit Area Redirect
            return RedirectToAction("Index", "MassTimingAdmin", new { area = "Admin" });
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            _uow.MassSchedules.Delete(id);
            LogAudit("DELETE", "massTiming", id);
            return JsonOk(message: "Schedule deleted.");
        }
    }
}