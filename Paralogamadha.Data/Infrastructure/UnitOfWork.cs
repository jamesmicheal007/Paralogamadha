// ============================================================
//  Paralogamadha.Data / Infrastructure / UnitOfWork.cs
// ============================================================

using Paralogamadha.Core.Interfaces;
using Paralogamadha.Data.Repositories;

namespace Paralogamadha.Data.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private IUserRepository          _users;
        private IRoleRepository          _roles;
        private ILanguageRepository      _languages;
        private ITranslationRepository   _translations;
        private ISiteSettingRepository   _siteSettings;
        private IPageMetaRepository      _pageMeta;
        private IHeroSlideRepository     _heroSlides;
        private IAnnouncementRepository  _announcements;
        private IQuickLinkRepository     _quickLinks;
        private IMassScheduleRepository  _massSchedules;
        private IPrayerRequestRepository _prayerRequests;
        private ITestimonialRepository   _testimonials;
        private IDailyContentRepository  _dailyContent;
        private IGalleryRepository       _gallery;
        private IPriestRepository        _priests;
        private IRoomRepository          _rooms;
        private IRoomBookingRepository   _roomBookings;
        private IDonationRepository      _donations;
        private IDashboardRepository     _dashboard;
        private ISongsRepository         _songs;

        public IUserRepository          Users          => _users          ??= new UserRepository();
        public IRoleRepository          Roles          => _roles          ??= new RoleRepository();
        public ILanguageRepository      Languages      => _languages      ??= new LanguageRepository();
        public ITranslationRepository   Translations   => _translations   ??= new TranslationRepository();
        public ISiteSettingRepository   SiteSettings   => _siteSettings   ??= new SiteSettingRepository();
        public IPageMetaRepository      PageMeta       => _pageMeta       ??= new PageMetaRepository();
        public IHeroSlideRepository     HeroSlides     => _heroSlides     ??= new HeroSlideRepository();
        public IAnnouncementRepository  Announcements  => _announcements  ??= new AnnouncementRepository();
        public IQuickLinkRepository     QuickLinks     => _quickLinks     ??= new QuickLinkRepository();
        public IMassScheduleRepository  MassSchedules  => _massSchedules  ??= new MassScheduleRepository();
        public IPrayerRequestRepository PrayerRequests => _prayerRequests ??= new PrayerRequestRepository();
        public ITestimonialRepository   Testimonials   => _testimonials   ??= new TestimonialRepository();
        public IDailyContentRepository  DailyContent   => _dailyContent   ??= new DailyContentRepository();
        public IGalleryRepository       Gallery        => _gallery        ??= new GalleryRepository();
        public IPriestRepository        Priests        => _priests        ??= new PriestRepository();
        public IRoomRepository          Rooms          => _rooms          ??= new RoomRepository();
        public IRoomBookingRepository   RoomBookings   => _roomBookings   ??= new RoomBookingRepository();
        public IDonationRepository      Donations      => _donations      ??= new DonationRepository();
        public IDashboardRepository     Dashboard      => _dashboard      ??= new DashboardRepository();
        public ISongsRepository         Songs          => _songs          ??= new SongsRepository();
        public void Dispose() { /* Dapper uses short-lived connections; no pooled connection to release */ }
    }
}
