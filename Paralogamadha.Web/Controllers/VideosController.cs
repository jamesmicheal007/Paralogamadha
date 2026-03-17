namespace Paralogamadha.Web.Controllers
{
    using Paralogamadha.Core.Interfaces;
    using Paralogamadha.Data.Repositories;

    public class VideosController : BaseController
    {
        private readonly IVideosRepository _videosRepo;

        public VideosController(IUnitOfWork uow, ITranslationService t, ISeoService seo, IVideosRepository videosRepo)
            : base(uow, t, seo) => _videosRepo = videosRepo;

        public System.Web.Mvc.ActionResult Index(int? categoryId = null)
        {
            SetPageMeta("videos");
            ViewBag.Categories = _videosRepo.GetCategories();
            var videos = categoryId.HasValue
                ? _videosRepo.GetByCategory(categoryId.Value, CurrentLanguageId)
                : _videosRepo.GetAll(CurrentLanguageId);
            return View(videos);
        }
    }
}