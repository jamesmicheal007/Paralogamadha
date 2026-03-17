// ============================================================
//  Paralogamadha.Core / Interfaces / IRepositories.cs
// ============================================================

using System;
using System.Collections.Generic;
using Paralogamadha.Core.Models;

namespace Paralogamadha.Core.Interfaces
{
    // ── Base ─────────────────────────────────────────────────
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetById(int id);
        int Upsert(T entity);
        bool Delete(int id);
    }

    // ── Auth ─────────────────────────────────────────────────
    public interface IUserRepository
    {
        ApplicationUser GetByUsername(string username);
        ApplicationUser GetByEmail(string email);
        IEnumerable<ApplicationUser> GetAll();
        int Upsert(ApplicationUser user);
        void UpdateLoginSuccess(int userId);
        void UpdateLoginFailed(int userId);
        void InsertAuditLog(AuditLog log);
    }

    public interface IRoleRepository
    {
        IEnumerable<Role> GetAll();
        Role GetById(int id);
    }

    // ── Language & Translation ────────────────────────────────
    public interface ILanguageRepository
    {
        IEnumerable<Language> GetActive();
        Language GetByCode(string code);
        Language GetDefault();
    }

    public interface ITranslationRepository
    {
        string Get(string key, int languageId);
        Dictionary<string, string> GetByModule(string moduleKey, int languageId);
        void Upsert(string key, int languageId, string value, string moduleKey, bool isAutomatic = false);
    }

    // ── Site Settings ─────────────────────────────────────────
    public interface ISiteSettingRepository
    {
        IEnumerable<SiteSetting> GetAll();
        string GetValue(string key);
        void Upsert(string key, string value, int updatedBy);
    }

    public interface IPageMetaRepository
    {
        PageMeta GetByKey(string pageKey, int languageId);
        int Upsert(PageMeta meta);
        IEnumerable<PageMeta> GetAll();
    }

    // ── Home ─────────────────────────────────────────────────
    public interface IHeroSlideRepository
    {
        IEnumerable<HeroSlide> GetActive(int languageId);
        IEnumerable<HeroSlide> GetAll();
        HeroSlide GetById(int id);
        int Upsert(HeroSlide slide);
        bool Delete(int id);
    }

    public interface IAnnouncementRepository
    {
        IEnumerable<Announcement> GetActive(int languageId, int top = 5);
        IEnumerable<Announcement> GetAll();
        Announcement GetById(int id);
        int Upsert(Announcement ann);
        bool Delete(int id);
    }

    public interface IQuickLinkRepository
    {
        IEnumerable<QuickLink> GetActive();
        IEnumerable<QuickLink> GetAll();
        int Upsert(QuickLink link);
        bool Delete(int id);
    }

    // ── Mass Timing ───────────────────────────────────────────
    public interface IMassScheduleRepository
    {
        IEnumerable<MassSchedule> GetByDay(byte dayOfWeek, int languageId);
        IEnumerable<MassSchedule> GetAll();
        IEnumerable<SpecialMass> GetUpcoming(int languageId, int days = 30);
        MassSchedule GetById(int id);
        int Upsert(MassSchedule schedule);
        bool Delete(int id);
    }

    // ── Prayer Requests ───────────────────────────────────────
    public interface IPrayerRequestRepository
    {
        IEnumerable<PrayerRequest> GetAll(byte? statusId = null);
        PrayerRequest GetById(int id);
        int Insert(PrayerRequest request);
        void Review(int requestId, byte statusId, string adminNotes, int reviewedBy);
        bool Delete(int id);
    }

    // ── Testimonials ──────────────────────────────────────────
    public interface ITestimonialRepository
    {
        IEnumerable<Testimonial> GetApproved(int languageId, bool featuredOnly = false);
        IEnumerable<Testimonial> GetAll();
        Testimonial GetById(int id);
        int Insert(Testimonial t);
        void Review(int id, byte statusId, int reviewedBy);
        void ToggleFeatured(int id, bool featured);
        bool Delete(int id);
    }

    // ── Daily Content ─────────────────────────────────────────
    public interface IDailyContentRepository
    {
        FeastOfDay GetFeast(DateTime date, int languageId);
        ReadingOfDay GetReading(DateTime date, int languageId);
        ThoughtOfDay GetThought(DateTime date, int languageId);
        int UpsertFeast(FeastOfDay feast);
        int UpsertReading(ReadingOfDay reading);
        int UpsertThought(ThoughtOfDay thought);
    }

    // ── Gallery ───────────────────────────────────────────────
    public interface IGalleryRepository
    {
        IEnumerable<GalleryCategory> GetCategories();
        IEnumerable<GalleryAlbum> GetAlbums(int? categoryId = null);
        GalleryAlbum GetAlbumById(int id);
        IEnumerable<GalleryPhoto> GetPhotosByAlbum(int albumId);
        int UpsertAlbum(GalleryAlbum album);
        int AddPhoto(GalleryPhoto photo);
        bool DeleteAlbum(int id);
        bool DeletePhoto(int id);
    }

    // ── Priests ───────────────────────────────────────────────
    public interface IPriestRepository
    {
        IEnumerable<Priest> GetAll(int? languageId = null);
        IEnumerable<Priest> GetCurrent(int languageId);
        Priest GetById(int id);
        int Upsert(Priest priest);
        bool Delete(int id);
    }

    // ── Room Booking ──────────────────────────────────────────
    public interface IRoomRepository
    {
        IEnumerable<Room> GetActive();
        IEnumerable<Room> GetAll();
        Room GetById(int id);
        int Upsert(Room room);
    }

    public interface IRoomBookingRepository
    {
        IEnumerable<RoomBooking> GetAll(int? roomId = null, byte? statusId = null);
        RoomBooking GetById(int id);
        RoomBooking GetByRef(string bookingRef);
        bool HasConflict(int roomId, DateTime start, DateTime end, int? excludeId = null);
        (int NewId, string BookingRef) Insert(RoomBooking booking);
        void Review(int bookingId, byte statusId, string adminNotes, int reviewedBy);
    }

    // ── Donation ──────────────────────────────────────────────
    public interface IDonationRepository
    {
        IEnumerable<DonationCategory> GetCategories();
        IEnumerable<Donation> GetAll();
        Donation GetById(long id);
        long Insert(Donation donation);
        void UpdateStatus(long donationId, byte statusId, string paymentId, string signature);
    }

    // ── Dashboard ─────────────────────────────────────────────
    public interface IDashboardRepository
    {
        DashboardStats GetStats();
    }

    // ── Unit of Work ──────────────────────────────────────────
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository          Users          { get; }
        IRoleRepository          Roles          { get; }
        ILanguageRepository      Languages      { get; }
        ITranslationRepository   Translations   { get; }
        ISiteSettingRepository   SiteSettings   { get; }
        IPageMetaRepository      PageMeta       { get; }
        IHeroSlideRepository     HeroSlides     { get; }
        IAnnouncementRepository  Announcements  { get; }
        IQuickLinkRepository     QuickLinks     { get; }
        IMassScheduleRepository  MassSchedules  { get; }
        IPrayerRequestRepository PrayerRequests { get; }
        ITestimonialRepository   Testimonials   { get; }
        IDailyContentRepository  DailyContent   { get; }
        IGalleryRepository       Gallery        { get; }
        IPriestRepository        Priests        { get; }
        IRoomRepository          Rooms          { get; }
        IRoomBookingRepository   RoomBookings   { get; }
        IDonationRepository      Donations      { get; }
        IDashboardRepository     Dashboard      { get; }
    }
}

namespace Paralogamadha.Core.Interfaces
{
    using System.Web;

    // ── Services ─────────────────────────────────────────────
    public interface ITranslationService
    {
        string Get(string key, string languageCode, string fallback = "");
        Dictionary<string, string> GetModule(string moduleKey, string languageCode);
        void Set(string key, string languageCode, string value, string moduleKey);
    }

    public interface IAuthService
    {
        (bool Success, string Error, ApplicationUser User) Login(string username, string password, string ip);
        string HashPassword(string password, out string salt);
        bool VerifyPassword(string password, string hash, string salt);
    }

    public interface IFileUploadService
    {
        UploadResult UploadImage(HttpPostedFileBase file, string folder);
        UploadResult UploadAudio(HttpPostedFileBase file, string folder);
        bool Delete(string relativePath);
    }

    public interface IEmailService
    {
        System.Threading.Tasks.Task SendAsync(string to, string subject, string htmlBody);
        System.Threading.Tasks.Task SendBookingConfirmationAsync(Models.RoomBooking booking);
        System.Threading.Tasks.Task SendPrayerAcknowledgementAsync(Models.PrayerRequest prayer);
        System.Threading.Tasks.Task SendDonationReceiptAsync(Models.Donation donation);
    }

    public interface ISeoService
    {
        Models.PageMeta GetMeta(string pageKey, string languageCode);
    }

    // ── Upload Result ─────────────────────────────────────────
    public class UploadResult
    {
        public bool   Success      { get; set; }
        public string FilePath     { get; set; }
        public string ThumbnailPath{ get; set; }
        public string FileName     { get; set; }
        public string Error        { get; set; }
        public int    WidthPx      { get; set; }
        public int    HeightPx     { get; set; }
        public int    FileSizeKb   { get; set; }
    }
}
