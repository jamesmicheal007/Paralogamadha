namespace Paralogamadha.Web.Controllers
{
    using System.Linq;
    using Paralogamadha.Core.Interfaces;
    using Paralogamadha.Data.Repositories;

    public class VirtualTourController : BaseController
    {
        private readonly IVirtualTourRepository _tourRepo;

        public VirtualTourController(IUnitOfWork uow, ITranslationService t, ISeoService seo, IVirtualTourRepository tourRepo)
            : base(uow, t, seo) => _tourRepo = tourRepo;

        public System.Web.Mvc.ActionResult Index()
        {
            var scenes = _tourRepo.GetAllScenes().ToList();
            foreach (var s in scenes) s.Hotspots = _tourRepo.GetHotspots(s.SceneId);

            // Build Pannellum config JSON
            var config = new
            {
                @default = new { firstScene = scenes.FirstOrDefault()?.SceneId.ToString() ?? "" },
                scenes = scenes.ToDictionary(
                    s => s.SceneId.ToString(),
                    s => new
                    {
                        title = s.SceneName,
                        panorama = s.PanoramaImageUrl,
                        hotSpots = (s.Hotspots ?? System.Linq.Enumerable.Empty<Paralogamadha.Core.Models.TourHotspot>())
                            .Select(h => new { type = "scene", sceneId = h.TargetSceneId.ToString(), pitch = h.PitchDegree, yaw = h.YawDegree, text = h.Label })
                    })
            };

            ViewBag.TourConfig = Newtonsoft.Json.JsonConvert.SerializeObject(config);
            ViewBag.Scenes = scenes;
            return View();
        }
    }
}