using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Paralogamadha.Core.Interfaces;
using Paralogamadha.Core.Models;

namespace Paralogamadha.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IUnitOfWork _uow;
        protected readonly ITranslationService _t;
        protected readonly ISeoService _seo;

        protected string CurrentLang =>
            (HttpContext.Items["CurrentLang"] as string) ?? "en";

        protected int CurrentLanguageId =>
            _uow.Languages.GetByCode(CurrentLang)?.LanguageId ?? 1;

        protected BaseController(IUnitOfWork uow, ITranslationService t, ISeoService seo)
        {
            _uow = uow;
            _t = t;
            _seo = seo;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Expose to all views
            ViewBag.CurrentLang = CurrentLang;
            ViewBag.Languages = _uow.Languages.GetActive();
            ViewBag.QuickLinks = _uow.QuickLinks.GetActive();
            ViewBag.SiteTitle = _uow.SiteSettings.GetValue("site.title");
            ViewBag.SiteTagline = _uow.SiteSettings.GetValue("site.tagline");
            ViewBag.FacebookUrl = _uow.SiteSettings.GetValue("site.facebookUrl");
            ViewBag.YoutubeUrl = _uow.SiteSettings.GetValue("site.youtubeUrl");
            ViewBag.InstagramUrl = _uow.SiteSettings.GetValue("site.instagramUrl");

            base.OnActionExecuting(filterContext);
        }

        protected void SetPageMeta(string pageKey)
        {
            var meta = _seo.GetMeta(pageKey, CurrentLang);
            ViewBag.MetaTitle = meta?.MetaTitle ?? _uow.SiteSettings.GetValue("site.title");
            ViewBag.MetaDescription = meta?.MetaDescription ?? _uow.SiteSettings.GetValue("seo.defaultDescription");
            ViewBag.OGTitle = meta?.OGTitle ?? ViewBag.MetaTitle;
            ViewBag.OGDescription = meta?.OGDescription ?? ViewBag.MetaDescription;
            ViewBag.OGImageUrl = meta?.OGImageUrl ?? _uow.SiteSettings.GetValue("seo.defaultOGImage");
            ViewBag.SchemaJson = meta?.SchemaJson;
        }

        protected string ClientIp()
        {
            var forwarded = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            return !string.IsNullOrEmpty(forwarded)
                ? forwarded.Split(',')[0].Trim()
                : Request.ServerVariables["REMOTE_ADDR"];
        }

        protected JsonResult JsonSuccess(object data = null, string message = "Success") =>
            Json(new { success = true, message, data }, JsonRequestBehavior.AllowGet);

        protected JsonResult JsonError(string message) =>
            Json(new { success = false, message }, JsonRequestBehavior.AllowGet);
    }
}