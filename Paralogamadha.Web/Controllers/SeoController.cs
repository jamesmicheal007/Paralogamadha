using Paralogamadha.Core.Interfaces;
using System;
using System.Web.Mvc;

namespace Paralogamadha.Web.Controllers
{
    public class SeoController : BaseController
    {
        public SeoController(IUnitOfWork uow, ITranslationService t, ISeoService seo)
            : base(uow, t, seo) { }

        public ActionResult Sitemap()
        {
            Response.ContentType = "application/xml";
            var priests = _uow.Priests.GetAll();
            var albums = _uow.Gallery.GetAlbums();
            return View("Sitemap", Tuple.Create(priests, albums));
        }

        public ActionResult Robots()
        {
            var siteUrl = $"{Request.Url?.Scheme}://{Request.Url?.Host}";
            var content = $"User-agent: *\nAllow: /\nDisallow: /admin/\nSitemap: {siteUrl}/sitemap.xml";
            return Content(content, "text/plain");
        }
    }
}