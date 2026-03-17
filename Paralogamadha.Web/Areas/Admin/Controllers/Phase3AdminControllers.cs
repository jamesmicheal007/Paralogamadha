// ============================================================
//  Phase3AdminControllers.cs
//  Videos, Songs, LiveTV, VirtualTour, History, SiteSettings admins
// ============================================================

using System;
using System.Web.Mvc;
using Paralogamadha.Core.Interfaces;
using Paralogamadha.Core.Models;
using Paralogamadha.Data.Repositories;

namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    // ── Videos Admin ─────────────────────────────────────────
    public class VideosAdminController : AdminBaseController
    {
        private readonly IVideosRepository _videosRepo;

        public VideosAdminController(IUnitOfWork uow, IFileUploadService upload, IVideosRepository videosRepo)
            : base(uow, upload) => _videosRepo = videosRepo;

        public ActionResult Index()
        {
            ViewBag.Categories = _videosRepo.GetCategories();
            return View(_videosRepo.GetAll(null));
        }

        public ActionResult Create()
        {
            ViewBag.Categories = _videosRepo.GetCategories();
            ViewBag.Languages  = _uow.Languages.GetActive();
            return View(new Video { IsPublished = true });
        }

        public ActionResult Edit(int id)
        {
            ViewBag.Categories = _videosRepo.GetCategories();
            ViewBag.Languages  = _uow.Languages.GetActive();
            return View(_videosRepo.GetById(id) ?? new Video());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Save(Video model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _videosRepo.GetCategories();
                ViewBag.Languages  = _uow.Languages.GetActive();
                return View("Edit", model);
            }
            model.CreatedBy = CurrentUserId;
            var id = _videosRepo.Upsert(model);
            LogAudit(model.VideoId == 0 ? "CREATE" : "UPDATE", "videos", id);
            TempData["Success"] = "Video saved.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            _videosRepo.Delete(id);
            LogAudit("DELETE", "videos", id);
            return JsonOk(message: "Video deleted.");
        }
    }

    // ── Live TV Admin ─────────────────────────────────────────
    public class LiveTVAdminController : AdminBaseController
    {
        private readonly ILiveTVRepository _tvRepo;

        public LiveTVAdminController(IUnitOfWork uow, IFileUploadService upload, ILiveTVRepository tvRepo)
            : base(uow, upload) => _tvRepo = tvRepo;

        public ActionResult Index() => View(_tvRepo.GetAll());

        public ActionResult Create() => View(new LiveTVChannel { IsActive = true });

        public ActionResult Edit(int id) => View(_tvRepo.GetById(id) ?? new LiveTVChannel());

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Save(LiveTVChannel model)
        {
            if (!ModelState.IsValid) return View("Edit", model);
            model.UpdatedBy = CurrentUserId;
            var id = _tvRepo.Upsert(model);
            LogAudit(model.ChannelId == 0 ? "CREATE" : "UPDATE", "liveTV", id);
            TempData["Success"] = "Channel saved.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult ToggleLive(int id, bool isLive)
        {
            _tvRepo.ToggleLive(id, isLive, CurrentUserId);
            LogAudit("TOGGLE_LIVE", "liveTV", id, $"IsLive → {isLive}");
            return JsonOk(message: $"Channel is now {(isLive ? "LIVE" : "offline")}.");
        }
    }

    // ── Virtual Tour Admin ────────────────────────────────────
    public class VirtualTourAdminController : AdminBaseController
    {
        private readonly IVirtualTourRepository _tourRepo;

        public VirtualTourAdminController(IUnitOfWork uow, IFileUploadService upload, IVirtualTourRepository tourRepo)
            : base(uow, upload) => _tourRepo = tourRepo;

        public ActionResult Index() => View(_tourRepo.GetAllScenes());

        public ActionResult Scene(int id)
        {
            var scene     = _tourRepo.GetSceneById(id);
            var hotspots  = _tourRepo.GetHotspots(id);
            var allScenes = _tourRepo.GetAllScenes();
            ViewBag.Scene     = scene;
            ViewBag.AllScenes = allScenes;
            return View(hotspots);
        }

        public ActionResult Create() => View(new VirtualTourScene { IsPublished = true });

        public ActionResult Edit(int id) => View(_tourRepo.GetSceneById(id) ?? new VirtualTourScene());

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SaveScene(VirtualTourScene model)
        {
            if (!ModelState.IsValid) return View("Edit", model);
            if (Request.Files["PanoramaImage"]?.ContentLength > 0)
            {
                var r = _upload.UploadImage(Request.Files["PanoramaImage"], "tour");
                if (!r.Success) { ModelState.AddModelError("", r.Error); return View("Edit", model); }
                model.PanoramaImageUrl = r.FilePath;
            }
            if (Request.Files["Thumbnail"]?.ContentLength > 0)
            {
                var r = _upload.UploadImage(Request.Files["Thumbnail"], "tour/thumbs");
                if (r.Success) model.ThumbnailUrl = r.FilePath;
            }
            var id = _tourRepo.UpsertScene(model);
            LogAudit(model.SceneId == 0 ? "CREATE" : "UPDATE", "virtualTour", id);
            TempData["Success"] = "Scene saved.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult SaveHotspot(TourHotspot model)
        {
            var id = _tourRepo.UpsertHotspot(model);
            return JsonOk(new { id }, "Hotspot saved.");
        }

        [HttpPost]
        public JsonResult DeleteHotspot(int id)
        {
            _tourRepo.DeleteHotspot(id);
            return JsonOk(message: "Hotspot deleted.");
        }

        [HttpPost]
        public JsonResult DeleteScene(int id)
        {
            _tourRepo.DeleteScene(id);
            LogAudit("DELETE", "virtualTour", id);
            return JsonOk(message: "Scene deleted.");
        }
    }

    // ── History Admin ─────────────────────────────────────────
    public class HistoryAdminController : AdminBaseController
    {
        private readonly IHistoryRepository _historyRepo;

        public HistoryAdminController(IUnitOfWork uow, IFileUploadService upload, IHistoryRepository historyRepo)
            : base(uow, upload) => _historyRepo = historyRepo;

        public ActionResult Index()
        {
            ViewBag.Content  = _historyRepo.GetContent(1);
            ViewBag.Timeline = _historyRepo.GetTimeline(1);
            return View();
        }

        public ActionResult EditContent(int id = 0)
        {
            ViewBag.Languages = _uow.Languages.GetActive();
            return View(id > 0 ? _historyRepo.GetContentById(id) : new HistoryContent());
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public ActionResult SaveContent(HistoryContent model)
        {
            var sanitizer = new Ganss.Xss.HtmlSanitizer();
            model.BodyHtml  = sanitizer.Sanitize(model.BodyHtml ?? "");
            model.CreatedBy = CurrentUserId;
            var id = _historyRepo.UpsertContent(model);
            LogAudit(model.ContentId == 0 ? "CREATE" : "UPDATE", "history", id);
            TempData["Success"] = "History content saved.";
            return RedirectToAction("Index");
        }

        public ActionResult EditTimeline(int id = 0)
        {
            ViewBag.Languages = _uow.Languages.GetActive();
            return View(id > 0 ? _historyRepo.GetTimelineById(id) : new HistoryTimeline());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SaveTimeline(HistoryTimeline model)
        {
            if (Request.Files["Image"]?.ContentLength > 0)
            {
                var r = _upload.UploadImage(Request.Files["Image"], "history");
                if (r.Success) model.ImageUrl = r.FilePath;
            }
            var id = _historyRepo.UpsertTimeline(model);
            LogAudit(model.TimelineId == 0 ? "CREATE" : "UPDATE", "history", id);
            TempData["Success"] = "Timeline item saved.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult DeleteContent(int id) { _historyRepo.DeleteContent(id); return JsonOk(message: "Deleted."); }

        [HttpPost]
        public JsonResult DeleteTimeline(int id) { _historyRepo.DeleteTimeline(id); return JsonOk(message: "Deleted."); }
    }

    // ── Site Settings Admin ───────────────────────────────────
    [Authorize(Roles = "SuperAdmin")]
    public class SiteSettingsAdminController : AdminBaseController
    {
        public SiteSettingsAdminController(IUnitOfWork uow, IFileUploadService upload)
            : base(uow, upload) { }

        public ActionResult Index() => View(_uow.SiteSettings.GetAll());

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Save(System.Web.HttpRequestBase request)
        {
            var all = _uow.SiteSettings.GetAll();
            foreach (var setting in all)
            {
                var val = Request.Form[setting.SettingKey];
                if (val == null) continue;
                // For encrypted fields: only update if non-empty (blank = keep current)
                if (setting.IsEncrypted && string.IsNullOrEmpty(val)) continue;
                _uow.SiteSettings.Upsert(setting.SettingKey, val, CurrentUserId);
            }
            // Invalidate translation cache for site title / tagline changes
            System.Runtime.Caching.MemoryCache.Default.Remove("trans_sitesettings");
            TempData["Success"] = "Settings saved successfully.";
            LogAudit("UPDATE_SETTINGS", "siteSettings");
            return RedirectToAction("Index");
        }
    }
}
