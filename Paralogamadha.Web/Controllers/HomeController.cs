using System;
using System.Web.Mvc;
using Paralogamadha.Core.Interfaces;
using Paralogamadha.Web.Models;
namespace Paralogamadha.Web.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IUnitOfWork uow, ITranslationService t, ISeoService seo)
            : base(uow, t, seo) { }

        public ActionResult Index()
        {
            SetPageMeta("home");
            var today = DateTime.Today;
            var langId = CurrentLanguageId;

            var model = new HomeViewModel
            {
                HeroSlides = _uow.HeroSlides.GetActive(langId),
                Announcements = _uow.Announcements.GetActive(langId, 5),
                TodayMasses = _uow.MassSchedules.GetByDay((byte)today.DayOfWeek, langId),
                TodayFeast = _uow.DailyContent.GetFeast(today, langId),
                TodayThought = _uow.DailyContent.GetThought(today, langId),
                TodayReading = _uow.DailyContent.GetReading(today, langId),
                UpcomingMasses = _uow.MassSchedules.GetUpcoming(langId, 7),
                Testimonials = _uow.Testimonials.GetApproved(langId, featuredOnly: true),
            };

            return View(model);
        }
    }
}