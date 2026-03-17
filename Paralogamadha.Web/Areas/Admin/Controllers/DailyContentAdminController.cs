namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    using Paralogamadha.Core.Interfaces;
    using Paralogamadha.Core.Models;
    using System;
    using System.Web;
    using System.Web.Mvc;

    public class DailyContentAdminController : AdminBaseController
    {
        public DailyContentAdminController(IUnitOfWork uow, IFileUploadService upload)
            : base(uow, upload) { }

        // 1. FEAST -----------------------------------------------------------
        public ActionResult Feast(DateTime? date = null)
        {
            var d = date ?? DateTime.Today;
            var model = _uow.DailyContent.GetFeast(d, 1) ?? new FeastOfDay { FeastDate = d };
            return View("~/Areas/Admin/Views/DailyContent/Feast.cshtml", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SaveFeast(FeastOfDay model)
        {
            if (!ModelState.IsValid)
                return View("~/Areas/Admin/Views/DailyContent/Feast.cshtml", model);

            // C# 5 compatible null check
            var imgFile = Request.Files["Image"];
            if (imgFile != null && imgFile.ContentLength > 0)
            {
                var r = _upload.UploadImage(imgFile, "feast");
                if (r.Success) model.ImageUrl = r.FilePath;
            }

            model.CreatedBy = CurrentUserId.ToString();
            _uow.DailyContent.UpsertFeast(model);

            TempData["Success"] = "Feast saved.";
            return RedirectToAction("Feast", "DailyContentAdmin", new { area = "Admin", date = model.FeastDate.ToString("yyyy-MM-dd") });
        }

        // 2. READING ---------------------------------------------------------
        public ActionResult Reading(DateTime? date = null)
        {
            var d = date ?? DateTime.Today;
            var model = _uow.DailyContent.GetReading(d, 1) ?? new ReadingOfDay { ReadingDate = d };
            return View("~/Areas/Admin/Views/DailyContent/Reading.cshtml", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SaveReading(ReadingOfDay model)
        {
            if (!ModelState.IsValid)
                return View("~/Areas/Admin/Views/DailyContent/Reading.cshtml", model);

            model.CreatedBy = CurrentUserId;
            _uow.DailyContent.UpsertReading(model);

            TempData["Success"] = "Reading saved.";
            return RedirectToAction("Reading", "DailyContentAdmin", new { area = "Admin", date = model.ReadingDate.ToString("yyyy-MM-dd") });
        }

        // 3. THOUGHT ---------------------------------------------------------
        public ActionResult Thought(DateTime? date = null)
        {
            var d = date ?? DateTime.Today;
            var model = _uow.DailyContent.GetThought(d, 1) ?? new ThoughtOfDay { ThoughtDate = d };
            return View("~/Areas/Admin/Views/DailyContent/Thought.cshtml", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SaveThought(ThoughtOfDay model)
        {
            if (!ModelState.IsValid)
                return View("~/Areas/Admin/Views/DailyContent/Thought.cshtml", model);

            // C# 5 compatible null check
            var bgFile = Request.Files["BgImage"];
            if (bgFile != null && bgFile.ContentLength > 0)
            {
                var r = _upload.UploadImage(bgFile, "thoughts");
                if (r.Success) model.BackgroundImageUrl = r.FilePath;
            }

            model.CreatedBy = CurrentUserId;
            _uow.DailyContent.UpsertThought(model);

            TempData["Success"] = "Thought saved.";
            return RedirectToAction("Thought", "DailyContentAdmin", new { area = "Admin", date = model.ThoughtDate.ToString("yyyy-MM-dd") });
        }
    }
}