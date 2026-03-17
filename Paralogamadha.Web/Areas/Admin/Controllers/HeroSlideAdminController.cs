
// ============================================================
//  HeroSlideAdminController.cs
// ============================================================

using System.Web.Mvc;
using Paralogamadha.Core.Interfaces;
using Paralogamadha.Core.Models;

namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    public class HeroSlideAdminController : AdminBaseController
    {
        public HeroSlideAdminController(IUnitOfWork uow, IFileUploadService upload)
            : base(uow, upload) { }

        public ActionResult Index() => View(_uow.HeroSlides.GetAll());

        public ActionResult Create() => View(new HeroSlide());

        public ActionResult Edit(int id) => View(_uow.HeroSlides.GetById(id) ?? new HeroSlide());

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Save(HeroSlide model)
        {
            if (!ModelState.IsValid) return View("Edit", model);

            if (model.SlideId == 0 && Request.Files["BackgroundImage"]?.ContentLength > 0)
            {
                var result = _upload.UploadImage(Request.Files["BackgroundImage"], "slides");
                if (!result.Success) { ModelState.AddModelError("", result.Error); return View("Edit", model); }
                model.BackgroundImageUrl = result.FilePath;
            }

            model.CreatedBy = CurrentUserId.ToString();
            var id = _uow.HeroSlides.Upsert(model);
            LogAudit(model.SlideId == 0 ? "CREATE" : "UPDATE", "heroSlides", id);
            TempData["Success"] = "Hero slide saved successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            _uow.HeroSlides.Delete(id);
            LogAudit("DELETE", "heroSlides", id);
            return JsonOk(message: "Slide deleted.");
        }
    }
}