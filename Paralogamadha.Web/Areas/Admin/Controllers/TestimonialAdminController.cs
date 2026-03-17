namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    using Paralogamadha.Core.Interfaces;
    using System.Web.Mvc;

    public class TestimonialAdminController : AdminBaseController
    {
        public TestimonialAdminController(IUnitOfWork uow, IFileUploadService upload)
            : base(uow, upload) { }

        // 1. Explicit path for Index (List of all testimonials)
        public ActionResult Index() =>
            View("~/Areas/Admin/Views/Testimonials/Index.cshtml", _uow.Testimonials.GetAll());

        // 2. Explicit path for Detail (Reviewing a specific testimonial)
        public ActionResult Detail(int id)
        {
            var model = _uow.Testimonials.GetById(id);
            if (model == null) return HttpNotFound();

            return View("~/Areas/Admin/Views/Testimonials/Detail.cshtml", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult Review(int id, byte statusId)
        {
            try
            {
                _uow.Testimonials.Review(id, statusId, CurrentUserId);
                LogAudit("REVIEW", "testimonials", id, "Status -> " + statusId);
                return JsonOk(message: "Testimonial status updated.");
            }
            catch (System.Exception ex)
            {
                return JsonFail("Update failed: " + ex.Message);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult ToggleFeatured(int id, bool featured)
        {
            try
            {
                _uow.Testimonials.ToggleFeatured(id, featured);
                return JsonOk(message: featured ? "Marked as featured." : "Removed from featured.");
            }
            catch (System.Exception ex)
            {
                return JsonFail("Toggle failed: " + ex.Message);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult Delete(int id)
        {
            try
            {
                _uow.Testimonials.Delete(id);
                LogAudit("DELETE", "testimonials", id);
                return JsonOk(message: "Testimonial deleted.");
            }
            catch (System.Exception ex)
            {
                return JsonFail("Delete failed: " + ex.Message);
            }
        }
    }
}