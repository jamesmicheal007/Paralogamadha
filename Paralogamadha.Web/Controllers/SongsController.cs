using System.Web.Mvc;
using Paralogamadha.Core.Interfaces;

namespace Paralogamadha.Web.Controllers
{
    public class SongsController : BaseController
    {
        public SongsController(IUnitOfWork uow, ITranslationService t, ISeoService seo)
            : base(uow, t, seo) { }

        //public ActionResult Index(int? categoryId = null)
        //{
        //    SetPageMeta("songs");
        //    var langId = CurrentLanguageId;

        //    ViewBag.Categories = _uow.Songs.GetCategories(langId);

        //    var songs = categoryId.HasValue
        //        ? _uow.Songs.GetByCategory(categoryId.Value, langId)
        //        : _uow.Songs.GetAll(langId);

        //    return View(songs);
        //}
    }
}