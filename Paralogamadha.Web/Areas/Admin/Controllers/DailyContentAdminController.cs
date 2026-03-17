
// ============================================================
//  DailyContentAdminController.cs
// ============================================================

namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    using Paralogamadha.Core.Interfaces;
    using Paralogamadha.Core.Models;
    using System;
    using System.Web.Mvc;

    public class DailyContentAdminController : AdminBaseController
    {
        public DailyContentAdminController(IUnitOfWork uow, IFileUploadService upload)
            : base(uow, upload) { }

        public ActionResult Feast(DateTime? date = null)
        {
            var d = date ?? DateTime.Today;
            var model = _uow.DailyContent.GetFeast(d, 1) ?? new FeastOfDay { FeastDate = d };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SaveFeast(FeastOfDay model)
        {
            if (!ModelState.IsValid) return View("Feast", model);
            if (Request.Files["Image"]?.ContentLength > 0)
            {
                var r = _upload.UploadImage(Request.Files["Image"], "feast");
                if (r.Success) model.ImageUrl = r.FilePath;
            }
            model.CreatedBy = CurrentUserId.ToString();
            _uow.DailyContent.UpsertFeast(model);
            TempData["Success"] = "Feast saved.";
            return RedirectToAction("Feast");
        }

        public ActionResult Reading(DateTime? date = null)
        {
            var d = date ?? DateTime.Today;
            var model = _uow.DailyContent.GetReading(d, 1) ?? new ReadingOfDay { ReadingDate = d };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SaveReading(ReadingOfDay model)
        {
            if (!ModelState.IsValid) return View("Reading", model);
            _uow.DailyContent.UpsertReading(model);
            TempData["Success"] = "Reading saved.";
            return RedirectToAction("Reading");
        }

        public ActionResult Thought(DateTime? date = null)
        {
            var d = date ?? DateTime.Today;
            var model = _uow.DailyContent.GetThought(d, 1) ?? new ThoughtOfDay { ThoughtDate = d };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SaveThought(ThoughtOfDay model)
        {
            if (!ModelState.IsValid) return View("Thought", model);
            if (Request.Files["BgImage"]?.ContentLength > 0)
            {
                var r = _upload.UploadImage(Request.Files["BgImage"], "thoughts");
                if (r.Success) model.BackgroundImageUrl = r.FilePath;
            }
            model.CreatedBy = CurrentUserId;
            _uow.DailyContent.UpsertThought(model);
            TempData["Success"] = "Thought saved.";
            return RedirectToAction("Thought");
        }
    }
}