using Paralogamadha.Core.Interfaces;
using Paralogamadha.Core.Models;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Paralogamadha.Web.Controllers
{
    public class RoomBookingController : BaseController
    {
        private readonly IEmailService _email;

        public RoomBookingController(IUnitOfWork uow, ITranslationService t, ISeoService seo, IEmailService email)
            : base(uow, t, seo) => _email = email;

        public ActionResult Index()
        {
            SetPageMeta("roomBooking");
            ViewBag.Rooms = _uow.Rooms.GetActive();
            return View();
        }

        [HttpGet]
        public JsonResult GetAvailability(int roomId, string start, string end)
        {
            if (!System.DateTime.TryParse(start, out var s) || !System.DateTime.TryParse(end, out var e))
                return JsonError("Invalid dates.");

            var hasConflict = _uow.RoomBookings.HasConflict(roomId, s, e);
            return JsonSuccess(new { available = !hasConflict });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> Book(RoomBooking model)
        {
            if (!ModelState.IsValid)
                return JsonError("Please fill all required fields.");

            if (_uow.RoomBookings.HasConflict(model.RoomId, model.StartDateTime, model.EndDateTime))
                return JsonError("This room is not available for the selected time.");

            var (newId, bookingRef) = _uow.RoomBookings.Insert(model);
            model.BookingId = newId;
            model.BookingRef = bookingRef;
            model.RoomName = _uow.Rooms.GetById(model.RoomId)?.RoomName;
            model.StatusId = 1; // Pending

            try { await _email.SendBookingConfirmationAsync(model); } catch { }

            return JsonSuccess(new { bookingRef }, $"Booking submitted! Reference: {bookingRef}");
        }
    }
}