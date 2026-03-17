// ============================================================
//  MassTimingAdminController.cs
// ============================================================

namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    using Paralogamadha.Core.Interfaces;
    using Paralogamadha.Core.Models;
    using System.Web.Mvc;

    public class MassTimingAdminController : AdminBaseController
    {
        public MassTimingAdminController(IUnitOfWork uow, IFileUploadService upload)
            : base(uow, upload) { }

        public ActionResult Index() => View(_uow.MassSchedules.GetAll());
        public ActionResult Create() => View(new MassSchedule());
        public ActionResult Edit(int id) => View(_uow.MassSchedules.GetById(id) ?? new MassSchedule());

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Save(MassSchedule model)
        {
            if (!ModelState.IsValid) return View("Edit", model);
            model.CreatedBy = CurrentUserId.ToString();
            var id = _uow.MassSchedules.Upsert(model);
            LogAudit(model.ScheduleId == 0 ? "CREATE" : "UPDATE", "massTiming", id);
            TempData["Success"] = "Mass schedule saved.";
            return RedirectToAction("Index");
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