using Paralogamadha.Core.Interfaces;
using System.Web.Mvc;

namespace Paralogamadha.Web.Controllers
{
    public class MassTimingController : BaseController
    {
        public MassTimingController(IUnitOfWork uow, ITranslationService t, ISeoService seo)
            : base(uow, t, seo) { }

        public ActionResult Index()
        {
            SetPageMeta("massTiming");
            var langId = CurrentLanguageId;

            var model = new
            {
                WeeklySchedules = _uow.MassSchedules.GetAll(),
                SpecialMasses = _uow.MassSchedules.GetUpcoming(langId, 60),
            };

            return View(model);
        }
    }
}