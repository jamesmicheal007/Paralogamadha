using Paralogamadha.Core.Interfaces;
using Paralogamadha.Core.Models;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Paralogamadha.Web.Controllers
{
    public class PrayerController : BaseController
    {
        private readonly IEmailService _email;

        public PrayerController(IUnitOfWork uow, ITranslationService t, ISeoService seo, IEmailService email)
            : base(uow, t, seo) => _email = email;

        public ActionResult Index()
        {
            SetPageMeta("prayer");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Submit(PrayerRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Please fill all required fields." });

            // Rate limit: max 3 per hour per IP
            var ip = ClientIp();
            var rateCacheKey = $"prayer_rate_{ip}";
            var count = (int?)System.Runtime.Caching.MemoryCache.Default[rateCacheKey] ?? 0;
            if (count >= 3)
                return Json(new { success = false, message = "Too many requests. Please try again later." });

            System.Runtime.Caching.MemoryCache.Default.Set(rateCacheKey,
                count + 1, System.DateTimeOffset.Now.AddHours(1));

            model.IpAddress = ip;
            model.LanguageId = CurrentLanguageId;
            var id = _uow.PrayerRequests.Insert(model);

            if (!model.IsAnonymous)
            {
                var saved = _uow.PrayerRequests.GetById(id);
                try { await _email.SendPrayerAcknowledgementAsync(saved); } catch { }
            }

            return Json(new { success = true, message = "Your prayer request has been submitted. God bless you." });
        }
    }
}