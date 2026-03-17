namespace Paralogamadha.Web.Controllers
{
    using Paralogamadha.Core.Interfaces;
    using Paralogamadha.Data.Repositories;

    public class HistoryController : BaseController
    {
        private readonly IHistoryRepository _historyRepo;

        public HistoryController(IUnitOfWork uow, ITranslationService t, ISeoService seo, IHistoryRepository historyRepo)
            : base(uow, t, seo) => _historyRepo = historyRepo;

        public System.Web.Mvc.ActionResult Index()
        {
            SetPageMeta("history");
            ViewBag.Content = _historyRepo.GetContent(CurrentLanguageId);
            ViewBag.Timeline = _historyRepo.GetTimeline(CurrentLanguageId);
            return View();
        }
    }
}