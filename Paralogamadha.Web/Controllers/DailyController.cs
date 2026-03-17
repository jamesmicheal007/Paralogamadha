using Paralogamadha.Core.Interfaces;
using System.Web.Mvc;

namespace Paralogamadha.Web.Controllers
{
    public class DailyController : BaseController
    {
        public DailyController(IUnitOfWork uow, ITranslationService t, ISeoService seo)
            : base(uow, t, seo) { }

        public ActionResult Feast()
        {
            var today = System.DateTime.Today;
            var model = _uow.DailyContent.GetFeast(today, CurrentLanguageId);
            return View(model);
        }

        public ActionResult Reading()
        {
            var today = System.DateTime.Today;
            var model = _uow.DailyContent.GetReading(today, CurrentLanguageId);
            return View(model);
        }

        public ActionResult Thought()
        {
            var today = System.DateTime.Today;
            var model = _uow.DailyContent.GetThought(today, CurrentLanguageId);
            return View(model);
        }
    }
}