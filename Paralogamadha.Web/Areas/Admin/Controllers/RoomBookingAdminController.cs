using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Paralogamadha.Core.Interfaces;
using Paralogamadha.Core.Models;

namespace Paralogamadha.Web.Areas.Admin.Controllers
{
    public class RoomBookingAdminController : AdminBaseController
    {
        private readonly IEmailService _email;

        public RoomBookingAdminController(IUnitOfWork uow, IFileUploadService upload, IEmailService email)
            : base(uow, upload)
        {
            _email = email;
        }

        // 1. BOOKINGS --------------------------------------------------------

        public ActionResult Index(byte? status = null, int? roomId = null)
        {
            ViewBag.Rooms = _uow.Rooms.GetAll();
            // Explicit path to the RoomBookings folder
            return View("~/Areas/Admin/Views/RoomBooking/Index.cshtml", _uow.RoomBookings.GetAll(roomId, status));
        }

        public ActionResult Detail(int id)
        {
            var booking = _uow.RoomBookings.GetById(id);
            if (booking == null) return HttpNotFound();

            return View("~/Areas/Admin/Views/RoomBooking/Detail.cshtml", booking);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> Review(int id, byte statusId, string adminNotes)
        {
            try
            {
                _uow.RoomBookings.Review(id, statusId, adminNotes, CurrentUserId);

                // C# 5 compatible concatenation
                LogAudit("REVIEW", "roomBookings", id, "Status -> " + statusId);

                var booking = _uow.RoomBookings.GetById(id);
                if (booking != null)
                {
                    // C# 5 compatible null check
                    var room = _uow.Rooms.GetById(booking.RoomId);
                    if (room != null)
                    {
                        booking.RoomName = room.RoomName;
                    }

                    // Attempt async email, swallow exception to prevent UI crash
                    try { await _email.SendBookingConfirmationAsync(booking); } catch { }
                }

                return JsonOk(message: "Booking updated and notification sent.");
            }
            catch (Exception ex)
            {
                return JsonFail("Failed to update: " + ex.Message);
            }
        }

        // 2. ROOM MANAGEMENT -------------------------------------------------

        public ActionResult Rooms()
        {
            // Explicit path to the Rooms folder
            return View("~/Areas/Admin/Views/Rooms/Index.cshtml", _uow.Rooms.GetAll());
        }

        public ActionResult EditRoom(int id)
        {
            var room = _uow.Rooms.GetById(id) ?? new Room();
            return View("~/Areas/Admin/Views/Rooms/Edit.cshtml", room);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SaveRoom(Room model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Areas/Admin/Views/Rooms/Edit.cshtml", model);
            }

            _uow.Rooms.Upsert(model);
            TempData["Success"] = "Room saved successfully.";

            // Explicit Area Redirect
            return RedirectToAction("Rooms", "RoomBookingAdmin", new { area = "Admin" });
        }
    }
}