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
            : base(uow, upload)
        {
            _auth = auth;
        }

        // 1. Explicit path for Index
        public ActionResult Index()
        {
            return View("~/Areas/Admin/Views/Users/Index.cshtml", _uow.Users.GetAll());
        }

        public ActionResult Create()
        {
            ViewBag.Roles = _uow.Roles.GetAll();
            return View("~/Areas/Admin/Views/Users/Create.cshtml", new ApplicationUser());
        }

        public ActionResult Edit(int id)
        {
            ViewBag.Roles = _uow.Roles.GetAll();
            // Using a traditional Find or FirstOrDefault logic
            var user = _uow.Users.GetAll().FirstOrDefault(u => u.UserId == id);

            if (user == null) return HttpNotFound();

            return View("~/Areas/Admin/Views/Users/Edit.cshtml", user);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Save(ApplicationUser model, string password)
        {
            ViewBag.Roles = _uow.Roles.GetAll();

            if (!ModelState.IsValid)
            {
                string viewName = (model.UserId == 0) ? "Create" : "Edit";
                return View("~/Areas/Admin/Views/Users/" + viewName + ".cshtml", model);
            }

            // Handle New User Password Hashing
            if (model.UserId == 0)
            {
                if (string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError("", "Password is required for new users.");
                    return View("~/Areas/Admin/Views/Users/Create.cshtml", model);
                }

                string salt;
                model.PasswordHash = _auth.HashPassword(password, out salt);
                model.PasswordSalt = salt;
                model.IsActive = true; // Default for new users
            }
            else
            {
                // For Edit: If password is provided, update it; otherwise, keep existing
                if (!string.IsNullOrEmpty(password))
                {
                    string salt;
                    model.PasswordHash = _auth.HashPassword(password, out salt);
                    model.PasswordSalt = salt;
                }
                else
                {
                    // Prevent the Upsert from clearing the password if the form field was empty
                    var existing = _uow.Users.GetAll().FirstOrDefault(u => u.UserId == model.UserId);
                    if (existing != null)
                    {
                        model.PasswordHash = existing.PasswordHash;
                        model.PasswordSalt = existing.PasswordSalt;
                    }
                }
            }

            var id = _uow.Users.Upsert(model);

            // C# 5 compatible concatenation
            string auditAction = (model.UserId == 0) ? "CREATE_USER" : "UPDATE_USER";
            LogAudit(auditAction, "users", id);

            TempData["Success"] = "User saved successfully.";

            // Explicit Area Redirect
            return RedirectToAction("Index", "UserAdmin", new { area = "Admin" });
        }
    }
}