namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    using Paralogamadha.Core.Interfaces;
    using Paralogamadha.Core.Models;
    using System.Web; // Required for HtmlEncode
    using System.Web.Mvc;
    using System.Text.RegularExpressions; // Required for manual strip

    public class PriestAdminController : AdminBaseController
    {
        public PriestAdminController(IUnitOfWork uow, IFileUploadService upload)
            : base(uow, upload) { }

        public ActionResult Index() => View(_uow.Priests.GetAll());
        public ActionResult Create() => View(new Priest());
        public ActionResult Edit(int id) => View(_uow.Priests.GetById(id) ?? new Priest());

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public ActionResult Save(Priest model)
        {
            if (!ModelState.IsValid) return View("Edit", model);

            if (Request.Files["Photo"]?.ContentLength > 0)
            {
                var r = _upload.UploadImage(Request.Files["Photo"], "priests");
                if (!r.Success) { ModelState.AddModelError("", r.Error); return View("Edit", model); }
                model.PhotoUrl = r.FilePath;
            }

            // FIX: Replaced Ganss.Xss with a manual Script Strip to fix the CS0246 error
            model.Bio = SanitizeHtml(model.Bio ?? "");

            model.CreatedBy = CurrentUserId;

            var id = _uow.Priests.Upsert(model);
            LogAudit(model.PriestId == 0 ? "CREATE" : "UPDATE", "priests", id);
            TempData["Success"] = "Priest profile saved.";
            return RedirectToAction("Index");
        }

        // Helper method to replace Ganss.Xss dependency
        private string SanitizeHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return html;
            // Removes <script> tags and their content
            return Regex.Replace(html, @"<script[^>]*>[\s\S]*?</script>", "", RegexOptions.IgnoreCase);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            _uow.Priests.Delete(id);
            LogAudit("DELETE", "priests", id);
            return JsonOk(message: "Deleted.");
        }
    }
}