
// ============================================================
//  RoomBookingAdminController.cs
// ============================================================

using System.Threading.Tasks;

namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    using Paralogamadha.Core.Interfaces;
    using System.Web.Mvc;

    public class RoomBookingAdminController : AdminBaseController
    {
        private readonly IEmailService _email;

        public RoomBookingAdminController(IUnitOfWork uow, IFileUploadService upload, IEmailService email)
            : base(uow, upload) => _email = email;

        public ActionResult Index(byte? status = null, int? roomId = null)
        {
            ViewBag.Rooms = _uow.Rooms.GetAll();
            return View(_uow.RoomBookings.GetAll(roomId, status));
        }

        public ActionResult Detail(int id) => View(_uow.RoomBookings.GetById(id));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> Review(int id, byte statusId, string adminNotes)
        {
            _uow.RoomBookings.Review(id, statusId, adminNotes, CurrentUserId);
            LogAudit("REVIEW", "roomBookings", id, $"Status → {statusId}");

            var booking = _uow.RoomBookings.GetById(id);
            if (booking != null)
            {
                booking.RoomName = _uow.Rooms.GetById(booking.RoomId)?.RoomName;
                try { await _email.SendBookingConfirmationAsync(booking); } catch { }
            }

            return JsonOk(message: "Booking updated and email sent.");
        }

        public ActionResult Rooms() => View(_uow.Rooms.GetAll());
        public ActionResult EditRoom(int id) => View(_uow.Rooms.GetById(id) ?? new Core.Models.Room());

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SaveRoom(Core.Models.Room model)
        {
            if (!ModelState.IsValid) return View("EditRoom", model);
            _uow.Rooms.Upsert(model);
            TempData["Success"] = "Room saved.";
            return RedirectToAction("Rooms");
        }
    }
}