using System.Web.Mvc;
using Paralogamadha.Core.Interfaces;
using Paralogamadha.Core.Models;

namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    public class SongsAdminController : AdminBaseController
    {
        public SongsAdminController(IUnitOfWork uow, IFileUploadService upload)
            : base(uow, upload) { }

        public ActionResult Index()
        {
            ViewBag.Categories = _uow.Songs.GetCategories(1);
            return View(_uow.Songs.GetAll(null));
        }

        public ActionResult Create()
        {
            ViewBag.Categories = _uow.Songs.GetCategories(1);
            ViewBag.Languages = _uow.Languages.GetActive();
            return View(new Song());
        }

        public ActionResult Edit(int id)
        {
            ViewBag.Categories = _uow.Songs.GetCategories(1);
            ViewBag.Languages = _uow.Languages.GetActive();
            return View(_uow.Songs.GetById(id) ?? new Song());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Save(Song model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _uow.Songs.GetCategories(1);
                ViewBag.Languages = _uow.Languages.GetActive();
                return View("Edit", model);
            }

            if (Request.Files["AudioFile"]?.ContentLength > 0)
            {
                var r = _upload.UploadAudio(Request.Files["AudioFile"], "songs");
                if (!r.Success) { ModelState.AddModelError("", r.Error); return View("Edit", model); }
                model.AudioFileUrl = r.FilePath;
            }

            model.CreatedBy = CurrentUserId;
            var id = _uow.Songs.Upsert(model);
            LogAudit(model.SongId == 0 ? "CREATE" : "UPDATE", "songs", id);
            TempData["Success"] = "Song saved.";
            return RedirectToAction("Index");
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