// ============================================================
//  Paralogamadha.Web / Areas / Admin / Controllers / AdminBaseController.cs
// ============================================================

using System.Web.Mvc;
using System.Web.Security;
using Paralogamadha.Core.Interfaces;
using Paralogamadha.Core.Models;

namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    [Authorize]
    public abstract class AdminBaseController : Controller
    {
        protected readonly IUnitOfWork _uow;
        protected readonly IFileUploadService _upload;

        protected int CurrentUserId
        {
            get
            {
                var ticket = ((FormsIdentity)User.Identity).Ticket;
                return int.TryParse(ticket.UserData, out int id) ? id : 0;
            }
        }

        protected AdminBaseController(IUnitOfWork uow, IFileUploadService upload)
        {
            _uow = uow;
            _upload = upload;
        }

        protected override void OnActionExecuting(ActionExecutingContext ctx)
        {
            ViewBag.ActiveModule = GetActiveModule(ctx.ActionDescriptor.ControllerDescriptor.ControllerName);
            ViewBag.CurrentUserId = CurrentUserId;
            ViewBag.Languages = _uow.Languages.GetActive();
            ViewBag.DashboardStats = _uow.Dashboard.GetStats();
            base.OnActionExecuting(ctx);
        }

        private string GetActiveModule(string controllerName) => controllerName.ToLower();

        protected JsonResult JsonOk(object data = null, string message = null) =>
            Json(new { success = true, message, data }, JsonRequestBehavior.AllowGet);

        protected JsonResult JsonFail(string message) =>
            Json(new { success = false, message }, JsonRequestBehavior.AllowGet);

        protected void LogAudit(string action, string module, int? entityId = null, string desc = null)
        {
            _uow.Users.InsertAuditLog(new AuditLog
            {
                UserId = CurrentUserId,
                Action = action,
                ModuleKey = module,
                EntityId = entityId,
                Description = desc,
                IpAddress = Request.UserHostAddress
            });
        }
    }
}