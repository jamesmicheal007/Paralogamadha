namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    using Paralogamadha.Core.Interfaces;
    using Paralogamadha.Core.Models;
    using System.Web;
    using System.Web.Mvc;
    using System.Text.RegularExpressions;

    public class PriestAdminController : AdminBaseController
    {
        public PriestAdminController(IUnitOfWork uow, IFileUploadService upload)
            : base(uow, upload) { }

        // 1. Explicit path for Index
        public ActionResult Index() =>
            View("~/Areas/Admin/Views/Priests/Index.cshtml", _uow.Priests.GetAll());

        // 2. Explicit path for Create (pointing to Edit.cshtml as shared form)
        public ActionResult Create() =>
            View("~/Areas/Admin/Views/Priests/Edit.cshtml", new Priest());

        // 3. Explicit path for Edit
        public ActionResult Edit(int id)
        {
            var model = _uow.Priests.GetById(id);
            if (model == null) return HttpNotFound();
            return View("~/Areas/Admin/Views/Priests/Edit.cshtml", model);
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public ActionResult Save(Priest model)
        {
            if (!ModelState.IsValid)
                return View("~/Areas/Admin/Views/Priests/Edit.cshtml", model);

            // C# 5 compatible null check (avoiding ?.)
            var photoFile = Request.Files["Photo"];
            if (photoFile != null && photoFile.ContentLength > 0)
            {
                var r = _upload.UploadImage(photoFile, "priests");
                if (!r.Success)
                {
                    ModelState.AddModelError("", r.Error);
                    return View("~/Areas/Admin/Views/Priests/Edit.cshtml", model);
                }
                model.PhotoUrl = r.FilePath;
            }

            model.Bio = SanitizeHtml(model.Bio ?? "");

            // Ensure CreatedBy is string to match common User ID storage
            model.CreatedBy = CurrentUserId;

            var id = _uow.Priests.Upsert(model);
            LogAudit(model.PriestId == 0 ? "CREATE" : "UPDATE", "priests", id);

            TempData["Success"] = "Priest profile saved.";

            // 4. Explicit Area Redirect
            return RedirectToAction("Index", "PriestAdmin", new { area = "Admin" });
        }

        private string SanitizeHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return html;
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