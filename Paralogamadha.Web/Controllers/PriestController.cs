using Paralogamadha.Core.Interfaces;
using System.Web.Mvc;

namespace Paralogamadha.Web.Controllers
{
    public class PriestController : BaseController
    {
        public PriestController(IUnitOfWork uow, ITranslationService t, ISeoService seo)
            : base(uow, t, seo) { }

        public ActionResult Index()
        {
            SetPageMeta("priests");
            var model = new
            {
                Current = _uow.Priests.GetCurrent(CurrentLanguageId),
                Past = _uow.Priests.GetAll(CurrentLanguageId),
            };
            return View(model);
        }
    }
}