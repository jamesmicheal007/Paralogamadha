using System.Web.Mvc;
using Paralogamadha.Core.Interfaces;
using Paralogamadha.Core.Models;

namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    public class HeroSlideAdminController : AdminBaseController
    {
        public HeroSlideAdminController(IUnitOfWork uow, IFileUploadService upload)
            : base(uow, upload) { }

        // 1. Explicit path for Index
        public ActionResult Index() =>
            View("~/Areas/Admin/Views/HeroSlides/Index.cshtml", _uow.HeroSlides.GetAll());

        // 2. Explicit path for Create (pointing to Edit.cshtml as the shared form)
        public ActionResult Create() =>
            View("~/Areas/Admin/Views/HeroSlides/Edit.cshtml", new HeroSlide());

        // 3. Explicit path for Edit
        public ActionResult Edit(int id)
        {
            var slide = _uow.HeroSlides.GetById(id);
            if (slide == null) return HttpNotFound();
            return View("~/Areas/Admin/Views/HeroSlides/Edit.cshtml", slide);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Save(HeroSlide model)
        {
            if (!ModelState.IsValid)
                return View("~/Areas/Admin/Views/HeroSlides/Edit.cshtml", model);

            // C# 5 compatible null check (avoiding ?.)
            var bgFile = Request.Files["BackgroundImage"];
            if (model.SlideId == 0 && bgFile != null && bgFile.ContentLength > 0)
            {
                var result = _upload.UploadImage(bgFile, "slides");
                if (!result.Success)
                {
                    ModelState.AddModelError("", result.Error);
                    return View("~/Areas/Admin/Views/HeroSlides/Edit.cshtml", model);
                }
                model.BackgroundImageUrl = result.FilePath;
            }

            model.CreatedBy = CurrentUserId.ToString();
            var id = _uow.HeroSlides.Upsert(model);

            LogAudit(model.SlideId == 0 ? "CREATE" : "UPDATE", "heroSlides", id);
            TempData["Success"] = "Hero slide saved successfully.";

            // 4. Explicit Area Redirect
            return RedirectToAction("Index", "HeroSlideAdmin", new { area = "Admin" });
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