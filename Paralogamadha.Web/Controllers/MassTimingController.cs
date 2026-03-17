using Paralogamadha.Core.Interfaces;
using Paralogamadha.Web.Models;
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

            var viewModel = new MassTimingViewModel
            {
                WeeklySchedules = _uow.MassSchedules.GetAll(), // Ensure this matches your Repo method
                SpecialMasses = _uow.MassSchedules.GetUpcoming(langId)
            };

            return View(viewModel);
        }
    }
}