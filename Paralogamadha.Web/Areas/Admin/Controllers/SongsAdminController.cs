using System.Web.Mvc;
using Paralogamadha.Core.Interfaces;
using Paralogamadha.Core.Models;

namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    public class SongsAdminController : AdminBaseController
    {
        public SongsAdminController(IUnitOfWork uow, IFileUploadService upload)
            : base(uow, upload) { }

        // 1. Explicit path for Index
        public ActionResult Index()
        {
            ViewBag.Categories = _uow.Songs.GetCategories(1);
            return View("~/Areas/Admin/Views/Songs/Index.cshtml", _uow.Songs.GetAll(null));
        }

        // 2. Explicit path for Create
        public ActionResult Create()
        {
            ViewBag.Categories = _uow.Songs.GetCategories(1);
            ViewBag.Languages = _uow.Languages.GetActive();
            return View("~/Areas/Admin/Views/Songs/Edit.cshtml", new Song());
        }

        // 3. Explicit path for Edit
        public ActionResult Edit(int id)
        {
            ViewBag.Categories = _uow.Songs.GetCategories(1);
            ViewBag.Languages = _uow.Languages.GetActive();
            var song = _uow.Songs.GetById(id);
            return View("~/Areas/Admin/Views/Songs/Edit.cshtml", song ?? new Song());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Save(Song model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _uow.Songs.GetCategories(1);
                ViewBag.Languages = _uow.Languages.GetActive();
                return View("~/Areas/Admin/Views/Songs/Edit.cshtml", model);
            }

            // FIX CS8026: Traditional null check for C# 5
            var audioFile = Request.Files["AudioFile"];
            if (audioFile != null && audioFile.ContentLength > 0)
            {
                var r = _upload.UploadAudio(audioFile, "songs");
                if (!r.Success)
                {
                    ModelState.AddModelError("", r.Error);
                    ViewBag.Categories = _uow.Songs.GetCategories(1);
                    ViewBag.Languages = _uow.Languages.GetActive();
                    return View("~/Areas/Admin/Views/Songs/Edit.cshtml", model);
                }
                model.AudioFileUrl = r.FilePath;
            }

            // Consistency fix: Ensure ID is string if your model expects it
            model.CreatedBy = CurrentUserId;

            var id = _uow.Songs.Upsert(model);

            // C# 5 string concatenation
            string action = (model.SongId == 0) ? "CREATE" : "UPDATE";
            LogAudit(action, "songs", id);

            TempData["Success"] = "Song saved.";

            // 4. Explicit Area Redirect
            return RedirectToAction("Index", "SongsAdmin", new { area = "Admin" });
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            _uow.Songs.Delete(id);
            LogAudit("DELETE", "songs", id);
            return JsonOk(message: "Song deleted.");
        }
    }
}