// ============================================================
//  UserAdminController.cs
// ============================================================

namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    using Paralogamadha.Core.Interfaces;
    using Paralogamadha.Core.Models;
    using System.Linq;
    using System.Web.Mvc;

    [Authorize(Roles = "SuperAdmin")]
    public class UserAdminController : AdminBaseController
    {
        private readonly IAuthService _auth;

        public UserAdminController(IUnitOfWork uow, IFileUploadService upload, IAuthService auth)
            : base(uow, upload) => _auth = auth;

        public ActionResult Index() => View(_uow.Users.GetAll());

        public ActionResult Create()
        {
            ViewBag.Roles = _uow.Roles.GetAll();
            return View(new ApplicationUser());
        }

        public ActionResult Edit(int id)
        {
            ViewBag.Roles = _uow.Roles.GetAll();
            return View(_uow.Users.GetAll().FirstOrDefault(u => u.UserId == id));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Save(ApplicationUser model, string password)
        {
            ViewBag.Roles = _uow.Roles.GetAll();
            if (!ModelState.IsValid) return View(model.UserId == 0 ? "Create" : "Edit", model);

            if (model.UserId == 0)
            {
                if (string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError("", "Password is required for new users.");
                    return View("Create", model);
                }
                model.PasswordHash = _auth.HashPassword(password, out string salt);
                model.PasswordSalt = salt;
            }

            var id = _uow.Users.Upsert(model);
            LogAudit(model.UserId == 0 ? "CREATE_USER" : "UPDATE_USER", "users", id);
            TempData["Success"] = "User saved.";
            return RedirectToAction("Index");
        }

        // Needed for LINQ FirstOrDefault in Edit action
        private System.Linq.Expressions.Expression<System.Func<ApplicationUser, bool>> ById(int id)
            => u => u.UserId == id;
    }
}
