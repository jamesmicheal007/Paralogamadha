namespace Paralogamadha.Web.Controllers
{
    using Paralogamadha.Core.Interfaces;

    public class GalleryController : BaseController
    {
        public GalleryController(IUnitOfWork uow, ITranslationService t, ISeoService seo)
            : base(uow, t, seo) { }

        public System.Web.Mvc.ActionResult Index(int? categoryId = null)
        {
            SetPageMeta("gallery");
            ViewBag.Categories = _uow.Gallery.GetCategories();
            return View(_uow.Gallery.GetAlbums(categoryId));
        }

        public System.Web.Mvc.ActionResult Album(int id)
        {
            var album = _uow.Gallery.GetAlbumById(id);
            if (album == null || !album.IsPublished) return HttpNotFound();
            ViewBag.Album = album;
            var photos = _uow.Gallery.GetPhotosByAlbum(id);
            return View(photos);
        }
    }
}