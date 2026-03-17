// ============================================================
//  Paralogamadha.Data / Repositories / Repositories.cs
//  All Dapper-based repository implementations
// ============================================================

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Paralogamadha.Core.Interfaces;
using Paralogamadha.Core.Models;
using Paralogamadha.Data.Infrastructure;

namespace Paralogamadha.Data.Repositories
{
    // ── User Repository ───────────────────────────────────────
    public class UserRepository : BaseRepository, IUserRepository
    {
        public ApplicationUser GetByUsername(string username)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<ApplicationUser>("auth.sp_GetUserByUsername",
                new { Username = username }, commandType: CommandType.StoredProcedure);
        }

        public ApplicationUser GetByEmail(string email)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<ApplicationUser>("auth.sp_GetUserByEmail",
                new { Email = email }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<ApplicationUser> GetAll()
        {
            using var c = CreateConnection();
            return c.Query<ApplicationUser>("auth.sp_GetAllUsers",
                commandType: CommandType.StoredProcedure);
        }

        public int Upsert(ApplicationUser user)
        {
            using var c = CreateConnection();
            return c.QuerySingle<int>("auth.sp_UpsertUser", new
            {
                user.UserId, user.Username, user.Email,
                user.PasswordHash, user.PasswordSalt,
                user.RoleId, user.FullName, user.IsActive
            }, commandType: CommandType.StoredProcedure);
        }

        public void UpdateLoginSuccess(int userId)
        {
            using var c = CreateConnection();
            c.Execute("auth.sp_UpdateLoginSuccess", new { UserId = userId },
                commandType: CommandType.StoredProcedure);
        }

        public void UpdateLoginFailed(int userId)
        {
            using var c = CreateConnection();
            c.Execute("auth.sp_UpdateLoginFailed", new { UserId = userId },
                commandType: CommandType.StoredProcedure);
        }

        public void InsertAuditLog(AuditLog log)
        {
            using var c = CreateConnection();
            c.Execute("auth.sp_InsertAuditLog", new
            {
                log.UserId, log.Action, log.ModuleKey,
                log.EntityId, log.Description, log.IpAddress
            }, commandType: CommandType.StoredProcedure);
        }
    }

    // ── Role Repository ───────────────────────────────────────
    public class RoleRepository : BaseRepository, IRoleRepository
    {
        public IEnumerable<Role> GetAll()
        {
            using var c = CreateConnection();
            return c.Query<Role>("SELECT * FROM auth.Roles WHERE IsActive = 1 ORDER BY RoleId");
        }

        public Role GetById(int id)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<Role>("SELECT * FROM auth.Roles WHERE RoleId = @Id",
                new { Id = id });
        }
    }

    // ── Language Repository ───────────────────────────────────
    public class LanguageRepository : BaseRepository, ILanguageRepository
    {
        public IEnumerable<Language> GetActive()
        {
            using var c = CreateConnection();
            return c.Query<Language>("cms.sp_GetActiveLanguages",
                commandType: CommandType.StoredProcedure);
        }

        public Language GetByCode(string code)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<Language>(
                "SELECT * FROM cms.Languages WHERE CultureCode = @Code AND IsActive = 1",
                new { Code = code });
        }

        public Language GetDefault()
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<Language>(
                "SELECT TOP 1 * FROM cms.Languages WHERE IsDefault = 1 AND IsActive = 1");
        }
    }

    // ── Translation Repository ────────────────────────────────
    public class TranslationRepository : BaseRepository, ITranslationRepository
    {
        public string Get(string key, int languageId)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<string>("cms.sp_GetTranslation",
                new { ResourceKey = key, LanguageId = languageId },
                commandType: CommandType.StoredProcedure);
        }

        public Dictionary<string, string> GetByModule(string moduleKey, int languageId)
        {
            using var c = CreateConnection();
            return c.Query("cms.sp_GetTranslationsByModule",
                new { ModuleKey = moduleKey, LanguageId = languageId },
                commandType: CommandType.StoredProcedure)
                .ToDictionary(r => (string)r.ResourceKey, r => (string)r.Value);
        }

        public void Upsert(string key, int languageId, string value, string moduleKey, bool isAutomatic = false)
        {
            using var c = CreateConnection();
            c.Execute("cms.sp_UpsertTranslation",
                new { ResourceKey = key, LanguageId = languageId, Value = value, ModuleKey = moduleKey, IsAutomatic = isAutomatic },
                commandType: CommandType.StoredProcedure);
        }
    }

    // ── Site Setting Repository ───────────────────────────────
    public class SiteSettingRepository : BaseRepository, ISiteSettingRepository
    {
        public IEnumerable<SiteSetting> GetAll()
        {
            using var c = CreateConnection();
            return c.Query<SiteSetting>("cms.sp_GetAllSettings",
                commandType: CommandType.StoredProcedure);
        }

        public string GetValue(string key)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<string>("cms.sp_GetSetting",
                new { SettingKey = key }, commandType: CommandType.StoredProcedure);
        }

        public void Upsert(string key, string value, int updatedBy)
        {
            using var c = CreateConnection();
            c.Execute("cms.sp_UpsertSetting",
                new { SettingKey = key, SettingValue = value, UpdatedBy = updatedBy },
                commandType: CommandType.StoredProcedure);
        }
    }

    // ── Page Meta Repository ──────────────────────────────────
    public class PageMetaRepository : BaseRepository, IPageMetaRepository
    {
        public PageMeta GetByKey(string pageKey, int languageId)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<PageMeta>(
                @"SELECT * FROM cms.PageMeta
                  WHERE PageKey = @PageKey AND LanguageId = @LanguageId",
                new { PageKey = pageKey, LanguageId = languageId });
        }

        public IEnumerable<PageMeta> GetAll()
        {
            using var c = CreateConnection();
            return c.Query<PageMeta>(
                "SELECT m.*, l.DisplayName AS LanguageName FROM cms.PageMeta m INNER JOIN cms.Languages l ON l.LanguageId = m.LanguageId ORDER BY m.PageKey");
        }

        public int Upsert(PageMeta meta)
        {
            using var c = CreateConnection();
            return c.QuerySingle<int>(@"
                MERGE cms.PageMeta AS target
                USING (SELECT @PageKey, @LanguageId) AS src (PageKey, LanguageId)
                ON target.PageKey = src.PageKey AND target.LanguageId = src.LanguageId
                WHEN MATCHED THEN UPDATE SET
                    MetaTitle = @MetaTitle, MetaDescription = @MetaDescription,
                    OGTitle = @OGTitle, OGDescription = @OGDescription,
                    OGImageUrl = @OGImageUrl, SchemaJson = @SchemaJson,
                    UpdatedAt = GETUTCDATE(), UpdatedBy = @UpdatedBy
                WHEN NOT MATCHED THEN INSERT
                    (PageKey, LanguageId, MetaTitle, MetaDescription, OGTitle, OGDescription, OGImageUrl, SchemaJson, UpdatedBy)
                VALUES (@PageKey, @LanguageId, @MetaTitle, @MetaDescription, @OGTitle, @OGDescription, @OGImageUrl, @SchemaJson, @UpdatedBy)
                OUTPUT INSERTED.MetaId;", meta);
        }
    }

    // ── Hero Slide Repository ─────────────────────────────────
    public class HeroSlideRepository : BaseRepository, IHeroSlideRepository
    {
        public IEnumerable<HeroSlide> GetActive(int languageId)
        {
            using var c = CreateConnection();
            return c.Query<HeroSlide>("cms.sp_GetActiveHeroSlides",
                new { LanguageId = languageId }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<HeroSlide> GetAll()
        {
            using var c = CreateConnection();
            return c.Query<HeroSlide>("cms.sp_GetAllHeroSlides",
                commandType: CommandType.StoredProcedure);
        }

        public HeroSlide GetById(int id)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<HeroSlide>(
                "SELECT * FROM cms.HeroSlides WHERE SlideId = @Id AND IsDeleted = 0",
                new { Id = id });
        }

        public int Upsert(HeroSlide slide)
        {
            using var c = CreateConnection();
            return c.QuerySingle<int>("cms.sp_UpsertHeroSlide", new
            {
                slide.SlideId, slide.LanguageId, slide.Title, slide.SubTitle,
                slide.BackgroundImageUrl, slide.ButtonText, slide.ButtonUrl,
                slide.SortOrder, slide.IsActive, slide.CreatedBy
            }, commandType: CommandType.StoredProcedure);
        }

        public bool Delete(int id)
        {
            using var c = CreateConnection();
            c.Execute("cms.sp_DeleteHeroSlide", new { SlideId = id },
                commandType: CommandType.StoredProcedure);
            return true;
        }
    }

    // ── Announcement Repository ───────────────────────────────
    public class AnnouncementRepository : BaseRepository, IAnnouncementRepository
    {
        public IEnumerable<Announcement> GetActive(int languageId, int top = 5)
        {
            using var c = CreateConnection();
            return c.Query<Announcement>("cms.sp_GetActiveAnnouncements",
                new { LanguageId = languageId, Top = top },
                commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<Announcement> GetAll()
        {
            using var c = CreateConnection();
            return c.Query<Announcement>("cms.sp_GetAllAnnouncements",
                commandType: CommandType.StoredProcedure);
        }

        public Announcement GetById(int id)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<Announcement>(
                "SELECT * FROM cms.Announcements WHERE AnnouncementId = @Id AND IsDeleted = 0",
                new { Id = id });
        }

        public int Upsert(Announcement ann)
        {
            using var c = CreateConnection();
            return c.QuerySingle<int>("cms.sp_UpsertAnnouncement", new
            {
                ann.AnnouncementId, ann.LanguageId, ann.Title, ann.Body,
                ann.Priority, ann.PublishDate, ann.ExpiryDate, ann.IsActive, ann.CreatedBy
            }, commandType: CommandType.StoredProcedure);
        }

        public bool Delete(int id)
        {
            using var c = CreateConnection();
            c.Execute("UPDATE cms.Announcements SET IsDeleted = 1 WHERE AnnouncementId = @Id",
                new { Id = id });
            return true;
        }
    }

    // ── Quick Link Repository ─────────────────────────────────
    public class QuickLinkRepository : BaseRepository, IQuickLinkRepository
    {
        public IEnumerable<QuickLink> GetActive()
        {
            using var c = CreateConnection();
            return c.Query<QuickLink>(
                "SELECT * FROM cms.QuickLinks WHERE IsActive = 1 ORDER BY SortOrder");
        }

        public IEnumerable<QuickLink> GetAll()
        {
            using var c = CreateConnection();
            return c.Query<QuickLink>("SELECT * FROM cms.QuickLinks ORDER BY SortOrder");
        }

        public int Upsert(QuickLink link)
        {
            using var c = CreateConnection();
            if (link.LinkId == 0)
            {
                return c.QuerySingle<int>(@"
                    INSERT INTO cms.QuickLinks (Title, Url, IconClass, SortOrder, IsActive)
                    OUTPUT INSERTED.LinkId
                    VALUES (@Title, @Url, @IconClass, @SortOrder, @IsActive)", link);
            }
            c.Execute(@"UPDATE cms.QuickLinks SET Title=@Title, Url=@Url,
                IconClass=@IconClass, SortOrder=@SortOrder, IsActive=@IsActive
                WHERE LinkId=@LinkId", link);
            return link.LinkId;
        }

        public bool Delete(int id)
        {
            using var c = CreateConnection();
            c.Execute("DELETE FROM cms.QuickLinks WHERE LinkId = @Id", new { Id = id });
            return true;
        }
    }

    // ── Mass Schedule Repository ──────────────────────────────
    public class MassScheduleRepository : BaseRepository, IMassScheduleRepository
    {
        public IEnumerable<MassSchedule> GetByDay(byte dayOfWeek, int languageId)
        {
            using var c = CreateConnection();
            return c.Query<MassSchedule>("cms.sp_GetMassSchedulesByDay",
                new { DayOfWeek = dayOfWeek, LanguageId = languageId },
                commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<MassSchedule> GetAll()
        {
            using var c = CreateConnection();
            return c.Query<MassSchedule>("cms.sp_GetAllMassSchedules",
                commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<SpecialMass> GetUpcoming(int languageId, int days = 30)
        {
            using var c = CreateConnection();
            return c.Query<SpecialMass>("cms.sp_GetUpcomingSpecialMasses",
                new { LanguageId = languageId, Days = days },
                commandType: CommandType.StoredProcedure);
        }

        public MassSchedule GetById(int id)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<MassSchedule>(
                "SELECT * FROM cms.MassSchedules WHERE ScheduleId = @Id AND IsDeleted = 0",
                new { Id = id });
        }

        public int Upsert(MassSchedule s)
        {
            using var c = CreateConnection();
            return c.QuerySingle<int>("cms.sp_UpsertMassSchedule", new
            {
                s.ScheduleId, s.LanguageId, s.DayOfWeek, s.MassTime,
                s.MassLanguage, s.Venue, s.Celebrant, s.Notes,
                s.IsActive, s.SortOrder, s.CreatedBy
            }, commandType: CommandType.StoredProcedure);
        }

        public bool Delete(int id)
        {
            using var c = CreateConnection();
            c.Execute("UPDATE cms.MassSchedules SET IsDeleted=1 WHERE ScheduleId=@Id", new { Id = id });
            return true;
        }
    }

    // ── Prayer Request Repository ─────────────────────────────
    public class PrayerRequestRepository : BaseRepository, IPrayerRequestRepository
    {
        public IEnumerable<PrayerRequest> GetAll(byte? statusId = null)
        {
            using var c = CreateConnection();
            return c.Query<PrayerRequest>("cms.sp_GetPrayerRequests",
                new { StatusId = statusId }, commandType: CommandType.StoredProcedure);
        }

        public PrayerRequest GetById(int id)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<PrayerRequest>(
                "SELECT * FROM cms.PrayerRequests WHERE RequestId = @Id AND IsDeleted = 0",
                new { Id = id });
        }

        public int Insert(PrayerRequest r)
        {
            using var c = CreateConnection();
            return c.QuerySingle<int>("cms.sp_InsertPrayerRequest", new
            {
                r.FullName, r.Email, r.Phone, r.PrayerCategory,
                r.PrayerText, r.IsAnonymous, r.IpAddress
            }, commandType: CommandType.StoredProcedure);
        }

        public void Review(int requestId, byte statusId, string adminNotes, int reviewedBy)
        {
            using var c = CreateConnection();
            c.Execute("cms.sp_ReviewPrayerRequest",
                new { RequestId = requestId, StatusId = statusId, AdminNotes = adminNotes, ReviewedBy = reviewedBy },
                commandType: CommandType.StoredProcedure);
        }

        public bool Delete(int id)
        {
            using var c = CreateConnection();
            c.Execute("UPDATE cms.PrayerRequests SET IsDeleted=1 WHERE RequestId=@Id", new { Id = id });
            return true;
        }
    }

    // ── Testimonial Repository ────────────────────────────────
    public class TestimonialRepository : BaseRepository, ITestimonialRepository
    {
        public IEnumerable<Testimonial> GetApproved(int languageId, bool featuredOnly = false)
        {
            using var c = CreateConnection();
            return c.Query<Testimonial>("cms.sp_GetApprovedTestimonials",
                new { LanguageId = languageId, FeaturedOnly = featuredOnly },
                commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<Testimonial> GetAll()
        {
            using var c = CreateConnection();
            return c.Query<Testimonial>(
                @"SELECT t.*, l.DisplayName AS LanguageName FROM cms.Testimonials t
                  INNER JOIN cms.Languages l ON l.LanguageId = t.LanguageId
                  WHERE t.IsDeleted = 0 ORDER BY t.CreatedAt DESC");
        }

        public Testimonial GetById(int id)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<Testimonial>(
                "SELECT * FROM cms.Testimonials WHERE TestimonialId = @Id AND IsDeleted = 0",
                new { Id = id });
        }

        public int Insert(Testimonial t)
        {
            using var c = CreateConnection();
            return c.QuerySingle<int>("cms.sp_InsertTestimonial", new
            {
                t.LanguageId, t.AuthorName, t.AuthorLocation, t.TestimonialText, t.Rating
            }, commandType: CommandType.StoredProcedure);
        }

        public void Review(int id, byte statusId, int reviewedBy)
        {
            using var c = CreateConnection();
            c.Execute(@"UPDATE cms.Testimonials SET StatusId=@StatusId,
                UpdatedAt=GETUTCDATE(), ReviewedBy=@ReviewedBy WHERE TestimonialId=@Id",
                new { Id = id, StatusId = statusId, ReviewedBy = reviewedBy });
        }

        public void ToggleFeatured(int id, bool featured)
        {
            using var c = CreateConnection();
            c.Execute("UPDATE cms.Testimonials SET FeaturedOnHome=@Featured WHERE TestimonialId=@Id",
                new { Id = id, Featured = featured });
        }

        public bool Delete(int id)
        {
            using var c = CreateConnection();
            c.Execute("UPDATE cms.Testimonials SET IsDeleted=1 WHERE TestimonialId=@Id", new { Id = id });
            return true;
        }
    }

    // ── Daily Content Repository ──────────────────────────────
    public class DailyContentRepository : BaseRepository, IDailyContentRepository
    {
        public FeastOfDay GetFeast(DateTime date, int languageId)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<FeastOfDay>("cms.sp_GetFeastByDate",
                new { FeastDate = date.Date, LanguageId = languageId },
                commandType: CommandType.StoredProcedure);
        }

        public ReadingOfDay GetReading(DateTime date, int languageId)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<ReadingOfDay>("cms.sp_GetReadingByDate",
                new { ReadingDate = date.Date, LanguageId = languageId },
                commandType: CommandType.StoredProcedure);
        }

        public ThoughtOfDay GetThought(DateTime date, int languageId)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<ThoughtOfDay>("cms.sp_GetThoughtByDate",
                new { ThoughtDate = date.Date, LanguageId = languageId },
                commandType: CommandType.StoredProcedure);
        }

        public int UpsertFeast(FeastOfDay f)
        {
            using var c = CreateConnection();
            return c.QuerySingle<int>(@"
                MERGE cms.FeastOfDay AS target
                USING (SELECT @FeastDate, @LanguageId) AS src (FeastDate, LanguageId)
                ON target.FeastDate = src.FeastDate AND target.LanguageId = src.LanguageId
                WHEN MATCHED THEN UPDATE SET SaintName=@SaintName, Description=@Description,
                    ImageUrl=@ImageUrl, IsAutomatic=@IsAutomatic, UpdatedAt=GETUTCDATE()
                WHEN NOT MATCHED THEN INSERT (LanguageId, FeastDate, SaintName, Description, ImageUrl, IsAutomatic)
                    VALUES (@LanguageId, @FeastDate, @SaintName, @Description, @ImageUrl, @IsAutomatic)
                OUTPUT INSERTED.FeastId;", f);
        }

        public int UpsertReading(ReadingOfDay r)
        {
            using var c = CreateConnection();
            return c.QuerySingle<int>(@"
                MERGE cms.ReadingOfDay AS target
                USING (SELECT @ReadingDate, @LanguageId) AS src (ReadingDate, LanguageId)
                ON target.ReadingDate = src.ReadingDate AND target.LanguageId = src.LanguageId
                WHEN MATCHED THEN UPDATE SET FirstReading=@FirstReading, FirstReadingRef=@FirstReadingRef,
                    Psalm=@Psalm, PsalmRef=@PsalmRef, SecondReading=@SecondReading,
                    SecondReadingRef=@SecondReadingRef, Gospel=@Gospel, GospelRef=@GospelRef,
                    LiturgicalColor=@LiturgicalColor, Source=@Source, UpdatedAt=GETUTCDATE()
                WHEN NOT MATCHED THEN INSERT (LanguageId, ReadingDate, FirstReading, FirstReadingRef,
                    Psalm, PsalmRef, SecondReading, SecondReadingRef, Gospel, GospelRef, LiturgicalColor, Source)
                VALUES (@LanguageId, @ReadingDate, @FirstReading, @FirstReadingRef,
                    @Psalm, @PsalmRef, @SecondReading, @SecondReadingRef, @Gospel, @GospelRef, @LiturgicalColor, @Source)
                OUTPUT INSERTED.ReadingId;", r);
        }

        public int UpsertThought(ThoughtOfDay t)
        {
            using var c = CreateConnection();
            return c.QuerySingle<int>(@"
                MERGE cms.ThoughtOfDay AS target
                USING (SELECT @ThoughtDate, @LanguageId) AS src (ThoughtDate, LanguageId)
                ON target.ThoughtDate = src.ThoughtDate AND target.LanguageId = src.LanguageId
                WHEN MATCHED THEN UPDATE SET Quote=@Quote, Attribution=@Attribution,
                    BackgroundImageUrl=@BackgroundImageUrl, UpdatedAt=GETUTCDATE()
                WHEN NOT MATCHED THEN INSERT (LanguageId, ThoughtDate, Quote, Attribution, BackgroundImageUrl)
                VALUES (@LanguageId, @ThoughtDate, @Quote, @Attribution, @BackgroundImageUrl)
                OUTPUT INSERTED.ThoughtId;", t);
        }
    }

    // ── Gallery Repository ────────────────────────────────────
    public class GalleryRepository : BaseRepository, IGalleryRepository
    {
        public IEnumerable<GalleryCategory> GetCategories()
        {
            using var c = CreateConnection();
            return c.Query<GalleryCategory>(
                "SELECT * FROM media.GalleryCategories WHERE IsActive=1 ORDER BY SortOrder");
        }

        public IEnumerable<GalleryAlbum> GetAlbums(int? categoryId = null)
        {
            using var c = CreateConnection();
            return c.Query<GalleryAlbum>(@"
                SELECT a.*, gc.CategoryName,
                       (SELECT COUNT(*) FROM media.GalleryPhotos p WHERE p.AlbumId=a.AlbumId AND p.IsDeleted=0) AS PhotoCount
                FROM media.GalleryAlbums a
                LEFT JOIN media.GalleryCategories gc ON gc.CategoryId = a.CategoryId
                WHERE a.IsDeleted=0 AND (@CategoryId IS NULL OR a.CategoryId=@CategoryId)
                ORDER BY a.EventDate DESC, a.SortOrder",
                new { CategoryId = categoryId });
        }

        public GalleryAlbum GetAlbumById(int id)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<GalleryAlbum>(
                "SELECT * FROM media.GalleryAlbums WHERE AlbumId=@Id AND IsDeleted=0", new { Id = id });
        }

        public IEnumerable<GalleryPhoto> GetPhotosByAlbum(int albumId)
        {
            using var c = CreateConnection();
            return c.Query<GalleryPhoto>(
                "SELECT * FROM media.GalleryPhotos WHERE AlbumId=@AlbumId AND IsDeleted=0 AND IsPublished=1 ORDER BY SortOrder",
                new { AlbumId = albumId });
        }

        public int UpsertAlbum(GalleryAlbum a)
        {
            using var c = CreateConnection();
            if (a.AlbumId == 0)
            {
                return c.QuerySingle<int>(@"
                    INSERT INTO media.GalleryAlbums (LanguageId, CategoryId, AlbumName, Description, CoverImageUrl, EventDate, IsPublished, SortOrder, CreatedBy)
                    OUTPUT INSERTED.AlbumId
                    VALUES (@LanguageId, @CategoryId, @AlbumName, @Description, @CoverImageUrl, @EventDate, @IsPublished, @SortOrder, @CreatedBy)", a);
            }
            c.Execute(@"UPDATE media.GalleryAlbums SET LanguageId=@LanguageId, CategoryId=@CategoryId,
                AlbumName=@AlbumName, Description=@Description, CoverImageUrl=@CoverImageUrl,
                EventDate=@EventDate, IsPublished=@IsPublished, SortOrder=@SortOrder, UpdatedAt=GETUTCDATE()
                WHERE AlbumId=@AlbumId", a);
            return a.AlbumId;
        }

        public int AddPhoto(GalleryPhoto p)
        {
            using var c = CreateConnection();
            return c.QuerySingle<int>(@"
                INSERT INTO media.GalleryPhotos (AlbumId, ImageUrl, ThumbnailUrl, Caption, AltText, SortOrder, FileSizeKb, Width, Height, IsPublished)
                OUTPUT INSERTED.PhotoId
                VALUES (@AlbumId, @ImageUrl, @ThumbnailUrl, @Caption, @AltText, @SortOrder, @FileSizeKb, @Width, @Height, @IsPublished)", p);
        }

        public bool DeleteAlbum(int id)
        {
            using var c = CreateConnection();
            c.Execute("UPDATE media.GalleryAlbums SET IsDeleted=1 WHERE AlbumId=@Id", new { Id = id });
            return true;
        }

        public bool DeletePhoto(int id)
        {
            using var c = CreateConnection();
            c.Execute("UPDATE media.GalleryPhotos SET IsDeleted=1 WHERE PhotoId=@Id", new { Id = id });
            return true;
        }
    }

    // ── Priest Repository ─────────────────────────────────────
    public class PriestRepository : BaseRepository, IPriestRepository
    {
        public IEnumerable<Priest> GetAll(int? languageId = null)
        {
            using var c = CreateConnection();
            return c.Query<Priest>(@"
                SELECT * FROM cms.Priests WHERE IsDeleted=0
                AND (@LanguageId IS NULL OR LanguageId=@LanguageId)
                ORDER BY IsCurrent DESC, StartDate DESC",
                new { LanguageId = languageId });
        }

        public IEnumerable<Priest> GetCurrent(int languageId)
        {
            using var c = CreateConnection();
            return c.Query<Priest>(
                "SELECT * FROM cms.Priests WHERE IsCurrent=1 AND IsDeleted=0 AND (LanguageId=@LId OR LanguageId=1) ORDER BY SortOrder",
                new { LId = languageId });
        }

        public Priest GetById(int id)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<Priest>(
                "SELECT * FROM cms.Priests WHERE PriestId=@Id AND IsDeleted=0", new { Id = id });
        }

        public int Upsert(Priest p)
        {
            using var c = CreateConnection();
            if (p.PriestId == 0)
            {
                return c.QuerySingle<int>(@"
                    INSERT INTO cms.Priests (LanguageId, FullName, Title, PhotoUrl, Bio, StartDate, EndDate, IsCurrent, SortOrder, CreatedBy)
                    OUTPUT INSERTED.PriestId
                    VALUES (@LanguageId, @FullName, @Title, @PhotoUrl, @Bio, @StartDate, @EndDate, @IsCurrent, @SortOrder, @CreatedBy)", p);
            }
            c.Execute(@"UPDATE cms.Priests SET LanguageId=@LanguageId, FullName=@FullName, Title=@Title,
                PhotoUrl=@PhotoUrl, Bio=@Bio, StartDate=@StartDate, EndDate=@EndDate,
                IsCurrent=@IsCurrent, SortOrder=@SortOrder, UpdatedAt=GETUTCDATE()
                WHERE PriestId=@PriestId", p);
            return p.PriestId;
        }

        public bool Delete(int id)
        {
            using var c = CreateConnection();
            c.Execute("UPDATE cms.Priests SET IsDeleted=1 WHERE PriestId=@Id", new { Id = id });
            return true;
        }
    }

    // ── Room Repository ───────────────────────────────────────
    public class RoomRepository : BaseRepository, IRoomRepository
    {
        public IEnumerable<Room> GetActive()
        {
            using var c = CreateConnection();
            return c.Query<Room>("SELECT * FROM cms.Rooms WHERE IsActive=1 AND IsDeleted=0 ORDER BY RoomName");
        }

        public IEnumerable<Room> GetAll()
        {
            using var c = CreateConnection();
            return c.Query<Room>("SELECT * FROM cms.Rooms WHERE IsDeleted=0 ORDER BY RoomName");
        }

        public Room GetById(int id)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<Room>("SELECT * FROM cms.Rooms WHERE RoomId=@Id AND IsDeleted=0", new { Id = id });
        }

        public int Upsert(Room r)
        {
            using var c = CreateConnection();
            if (r.RoomId == 0)
            {
                return c.QuerySingle<int>(@"
                    INSERT INTO cms.Rooms (RoomName, Capacity, Description, ImageUrl, HourlyRate, Amenities, IsActive)
                    OUTPUT INSERTED.RoomId
                    VALUES (@RoomName, @Capacity, @Description, @ImageUrl, @HourlyRate, @Amenities, @IsActive)", r);
            }
            c.Execute(@"UPDATE cms.Rooms SET RoomName=@RoomName, Capacity=@Capacity,
                Description=@Description, ImageUrl=@ImageUrl, HourlyRate=@HourlyRate,
                Amenities=@Amenities, IsActive=@IsActive WHERE RoomId=@RoomId", r);
            return r.RoomId;
        }
    }

    // ── Room Booking Repository ───────────────────────────────
    public class RoomBookingRepository : BaseRepository, IRoomBookingRepository
    {
        public IEnumerable<RoomBooking> GetAll(int? roomId = null, byte? statusId = null)
        {
            using var c = CreateConnection();
            return c.Query<RoomBooking>("cms.sp_GetRoomBookings",
                new { RoomId = roomId, StatusId = statusId },
                commandType: CommandType.StoredProcedure);
        }

        public RoomBooking GetById(int id)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<RoomBooking>(
                "SELECT * FROM cms.RoomBookings WHERE BookingId=@Id", new { Id = id });
        }

        public RoomBooking GetByRef(string bookingRef)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<RoomBooking>(
                "SELECT * FROM cms.RoomBookings WHERE BookingRef=@Ref", new { Ref = bookingRef });
        }

        public bool HasConflict(int roomId, DateTime start, DateTime end, int? excludeId = null)
        {
            using var c = CreateConnection();
            int count = c.QuerySingle<int>("cms.sp_CheckRoomConflict",
                new { RoomId = roomId, Start = start, End = end, ExcludeBookingId = excludeId },
                commandType: CommandType.StoredProcedure);
            return count > 0;
        }

        public (int NewId, string BookingRef) Insert(RoomBooking booking)
        {
            using var c = CreateConnection();
            var result = c.QuerySingle("cms.sp_InsertRoomBooking", new
            {
                booking.RoomId, booking.BookedByName, booking.Email,
                booking.Phone, booking.Purpose, booking.StartDateTime, booking.EndDateTime
            }, commandType: CommandType.StoredProcedure);
            return ((int)result.NewId, (string)result.BookingRef);
        }

        public void Review(int bookingId, byte statusId, string adminNotes, int reviewedBy)
        {
            using var c = CreateConnection();
            c.Execute("cms.sp_ReviewRoomBooking",
                new { BookingId = bookingId, StatusId = statusId, AdminNotes = adminNotes, ReviewedBy = reviewedBy },
                commandType: CommandType.StoredProcedure);
        }
    }

    // ── Donation Repository ───────────────────────────────────
    public class DonationRepository : BaseRepository, IDonationRepository
    {
        public IEnumerable<DonationCategory> GetCategories()
        {
            using var c = CreateConnection();
            return c.Query<DonationCategory>(
                "SELECT * FROM cms.DonationCategories WHERE IsActive=1 ORDER BY SortOrder");
        }

        public IEnumerable<Donation> GetAll()
        {
            using var c = CreateConnection();
            return c.Query<Donation>(@"
                SELECT d.*, dc.CategoryName FROM cms.Donations d
                INNER JOIN cms.DonationCategories dc ON dc.CategoryId = d.CategoryId
                ORDER BY d.CreatedAt DESC");
        }

        public Donation GetById(long id)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<Donation>(
                "SELECT * FROM cms.Donations WHERE DonationId=@Id", new { Id = id });
        }

        public long Insert(Donation d)
        {
            using var c = CreateConnection();
            return c.QuerySingle<long>(@"
                INSERT INTO cms.Donations (CategoryId, DonorName, Email, Phone, Amount, Currency, PaymentGateway, GatewayOrderId, StatusId, IsAnonymous, IpAddress)
                OUTPUT INSERTED.DonationId
                VALUES (@CategoryId, @DonorName, @Email, @Phone, @Amount, @Currency, @PaymentGateway, @GatewayOrderId, 1, @IsAnonymous, @IpAddress)", d);
        }

        public void UpdateStatus(long donationId, byte statusId, string paymentId, string signature)
        {
            using var c = CreateConnection();
            c.Execute(@"UPDATE cms.Donations SET StatusId=@StatusId,
                GatewayPaymentId=@PaymentId, GatewaySignature=@Signature,
                PaymentDate=CASE WHEN @StatusId=2 THEN GETUTCDATE() ELSE NULL END
                WHERE DonationId=@DonationId",
                new { DonationId = donationId, StatusId = statusId, PaymentId = paymentId, Signature = signature });
        }
    }

    // ── Dashboard Repository ──────────────────────────────────
    public class DashboardRepository : BaseRepository, IDashboardRepository
    {
        public DashboardStats GetStats()
        {
            using var c = CreateConnection();
            return c.QuerySingle<DashboardStats>("cms.sp_GetDashboardStats",
                commandType: CommandType.StoredProcedure);
        }
    }
}
