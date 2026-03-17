
// ============================================================
//  TestimonialAdminController.cs
// ============================================================

namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    using Paralogamadha.Core.Interfaces;
    using System.Web.Mvc;

    public class TestimonialAdminController : AdminBaseController
    {
        public TestimonialAdminController(IUnitOfWork uow, IFileUploadService upload)
            : base(uow, upload) { }

        public ActionResult Index() => View(_uow.Testimonials.GetAll());
        public ActionResult Detail(int id) => View(_uow.Testimonials.GetById(id));

        [HttpPost]
        public JsonResult Review(int id, byte statusId)
        {
            _uow.Testimonials.Review(id, statusId, CurrentUserId);
            LogAudit("REVIEW", "testimonials", id);
            return JsonOk();
        }

        [HttpPost]
        public JsonResult ToggleFeatured(int id, bool featured)
        {
            _uow.Testimonials.ToggleFeatured(id, featured);
            return JsonOk();
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            _uow.Testimonials.Delete(id);
            LogAudit("DELETE", "testimonials", id);
            return JsonOk(message: "Deleted.");
        }
    }
}