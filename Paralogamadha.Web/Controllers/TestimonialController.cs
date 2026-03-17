using Paralogamadha.Core.Interfaces;
using System.Web.Mvc;

namespace Paralogamadha.Web.Controllers
{
    public class TestimonialController : BaseController
    {
        public TestimonialController(IUnitOfWork uow, ITranslationService t, ISeoService seo)
            : base(uow, t, seo) { }

        public ActionResult Index()
        {
            var model = _uow.Testimonials.GetApproved(CurrentLanguageId);
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Submit(Core.Models.Testimonial model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false });

            model.LanguageId = CurrentLanguageId;
            _uow.Testimonials.Insert(model);
            return Json(new { success = true, message = "Thank you! Your testimonial will appear after review." });
        }
    }
}