namespace Paralogamadha.Web.Controllers
{
    using Paralogamadha.Core.Interfaces;
    using Paralogamadha.Data.Repositories;

    public class LiveTVController : BaseController
    {
        private readonly ILiveTVRepository _tvRepo;

        public LiveTVController(IUnitOfWork uow, ITranslationService t, ISeoService seo, ILiveTVRepository tvRepo)
            : base(uow, t, seo) => _tvRepo = tvRepo;

        public System.Web.Mvc.ActionResult Index(int? channel = null)
        {
            var channels = _tvRepo.GetActive();
            return View(channels);
        }
    }
}