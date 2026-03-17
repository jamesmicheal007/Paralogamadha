namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using Paralogamadha.Core.Interfaces;
    using Paralogamadha.Core.Models;

    public class GalleryAdminController : AdminBaseController
    {
        public GalleryAdminController(IUnitOfWork uow, IFileUploadService upload)
            : base(uow, upload) { }

        // 1. Explicit paths for Views to bypass folder naming mismatch
        public ActionResult Index()
        {
            ViewBag.Categories = _uow.Gallery.GetCategories();
            return View("~/Areas/Admin/Views/Gallery/Index.cshtml", _uow.Gallery.GetAlbums());
        }

        public ActionResult AlbumDetail(int id)
        {
            ViewBag.Album = _uow.Gallery.GetAlbumById(id);
            return View("~/Areas/Admin/Views/Gallery/AlbumDetail.cshtml", _uow.Gallery.GetPhotosByAlbum(id));
        }

        public ActionResult CreateAlbum()
        {
            ViewBag.Categories = _uow.Gallery.GetCategories();
            return View("~/Areas/Admin/Views/Gallery/EditAlbum.cshtml", new GalleryAlbum());
        }

        public ActionResult EditAlbum(int id)
        {
            ViewBag.Categories = _uow.Gallery.GetCategories();
            return View("~/Areas/Admin/Views/Gallery/EditAlbum.cshtml", _uow.Gallery.GetAlbumById(id));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SaveAlbum(GalleryAlbum model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _uow.Gallery.GetCategories();
                return View("~/Areas/Admin/Views/Gallery/EditAlbum.cshtml", model);
            }

            // C# 5 compatible null check (fixing CS8026)
            var coverFile = Request.Files["CoverImage"];
            if (coverFile != null && coverFile.ContentLength > 0)
            {
                var r = _upload.UploadImage(coverFile, "gallery/covers");
                if (r.Success) model.CoverImageUrl = r.FilePath;
            }

            model.CreatedBy = CurrentUserId.ToString();
            var id = _uow.Gallery.UpsertAlbum(model);

            LogAudit(model.AlbumId == 0 ? "CREATE" : "UPDATE", "gallery", id);
            TempData["Success"] = "Album saved.";

            // Explicit Area Redirect
            return RedirectToAction("Index", "GalleryAdmin", new { area = "Admin" });
        }

        [HttpPost]
        public JsonResult UploadPhotos(int albumId)
        {
            var uploaded = new List<object>();
            foreach (string key in Request.Files)
            {
                var file = Request.Files[key];
                if (file == null || file.ContentLength == 0) continue;

                // C# 5 string concatenation instead of $""
                var r = _upload.UploadImage(file, "gallery/" + albumId);
                if (!r.Success) continue;

                var photo = new GalleryPhoto
                {
                    AlbumId = albumId,
                    ImageUrl = r.FilePath,
                    ThumbnailUrl = r.ThumbnailPath,
                    Width = r.WidthPx,
                    Height = r.HeightPx,
                    FileSizeKb = r.FileSizeKb,
                    IsPublished = true
                };
                var photoId = _uow.Gallery.AddPhoto(photo);
                uploaded.Add(new { photoId, imageUrl = r.FilePath, thumbnailUrl = r.ThumbnailPath });
            }
            return JsonOk(uploaded);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult DeletePhoto(int id)
        {
            _uow.Gallery.DeletePhoto(id);
            return JsonOk(message: "Photo deleted.");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult DeleteAlbum(int id)
        {
            _uow.Gallery.DeleteAlbum(id);
            LogAudit("DELETE", "gallery", id);
            return JsonOk(message: "Album deleted.");
        }
    }
}