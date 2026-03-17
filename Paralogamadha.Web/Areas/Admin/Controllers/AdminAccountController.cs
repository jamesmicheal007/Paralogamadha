// ============================================================
//  Areas/Admin/Controllers/AdminAccountController.cs
//  Login, logout — does NOT inherit AdminBaseController
//  (avoids DI failures during auth failure scenarios)
// ============================================================

using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Paralogamadha.Core.Interfaces;

namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    public class AdminAccountController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IAuthService _auth;

        public AdminAccountController(IUnitOfWork uow, IAuthService auth)
        {
            _uow = uow;
            _auth = auth;
        }

        // GET /admin/adminaccount/login
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST /admin/adminaccount/login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password, string returnUrl)
        {
            //// Example using your AuthService logic
            //var authService = new AuthService(null); // UOW not needed for hashing
            //string newSalt;
            //string newHash = authService.HashPassword("admin", out newSalt);
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Username and password are required.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            var ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"]?.Split(',')[0]?.Trim()
                  ?? Request.ServerVariables["REMOTE_ADDR"];

            var (success, error, user) = _auth.Login(username.Trim(), password, ip);

            if (!success)
            {
                ViewBag.Error = error;
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            //// Issue Forms auth ticket — store UserId in UserData
            //var ticket = new FormsAuthenticationTicket(
            //    version: 1,
            //    name: user.Username,
            //    issueDate: DateTime.UtcNow,
            //    expiration: DateTime.UtcNow.AddMinutes(30),
            //    isPersistent: false,
            //    userData: user.UserId.ToString(),
            //    cookiePath: FormsAuthentication.FormsCookiePath
            //);
            // Get the user's role from your database first
            string userRole = user.RoleName; // Replace with actual role from _uow.Users

            var ticket = new FormsAuthenticationTicket(
                version: 1,
                name: user.Username,
                issueDate: DateTime.UtcNow,
                expiration: DateTime.UtcNow.AddMinutes(120), // 2 hours
                isPersistent: false,
                userData: user.UserId + "|" + userRole, // Store ID and Role separated by a pipe
                cookiePath: FormsAuthentication.FormsCookiePath
            );
            var encrypted = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted)
            {
                HttpOnly = true,
                Secure = Request.IsSecureConnection,
                SameSite = SameSiteMode.Strict,
                Path = FormsAuthentication.FormsCookiePath
            };
            Response.Cookies.Add(cookie);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        // GET /admin/adminaccount/logout
        [HttpGet]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "AdminAccount", new { area = "Admin" });
        }

        // GET /admin/adminaccount/accessdenied
        [HttpGet]
        [AllowAnonymous]
        public ActionResult AccessDenied()
        {
            Response.StatusCode = 403;
            return View();
        }
    }
}
