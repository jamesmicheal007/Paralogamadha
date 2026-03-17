// ============================================================
//  AdminAccountController.cs — Login/Logout
// ============================================================

using Paralogamadha.Core.Interfaces;
using Paralogamadha.Services;
using System;
using System.Web.Mvc;
using System.Web.Security;

namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    [AllowAnonymous]
    public class AdminAccountController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IAuthService _auth;

        public AdminAccountController(IUnitOfWork uow, IAuthService auth)
        {
            _uow = uow;
            _auth = auth;
        }

        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard");
            ViewBag.ReturnUrl = returnUrl;
            return View();  
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password, string returnUrl)
        {
            // Example using your AuthService logic
            var authService = new AuthService(null); // UOW not needed for hashing
            string newSalt;
            string newHash = authService.HashPassword("admin", out newSalt);

            var ip = Request.UserHostAddress;
            var (success, error, user) = _auth.Login(username, password, ip);

            if (!success)
            {
                ViewBag.Error = error;
                return View();
            }

            var ticket = new FormsAuthenticationTicket(
                version: 1,
                name: user.Username,
                issueDate: DateTime.UtcNow,
                expiration: DateTime.UtcNow.AddMinutes(30),
                isPersistent: false,
                userData: user.UserId.ToString()
            );

            var encrypted = FormsAuthentication.Encrypt(ticket);
            Response.Cookies.Add(new System.Web.HttpCookie(FormsAuthentication.FormsCookieName, encrypted)
            {
                HttpOnly = true,
                Secure = true,
                SameSite = System.Web.SameSiteMode.Strict
            });

            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}