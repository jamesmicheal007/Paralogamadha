// ============================================================
//  Paralogamadha.Core / Models / DomainModels.cs
//  All domain entity models - one file for Phase 1 clarity
// ============================================================

using System;
using System.Collections.Generic;

namespace Paralogamadha.Core.Models
{
    // ── Auth ─────────────────────────────────────────────────
    public class ApplicationUser
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Permissions { get; set; }  // JSON
        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int FailedLoginCount { get; set; }
        public DateTime? LockedUntil { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Permissions { get; set; }
        public bool IsActive { get; set; }
    }

    public class AuditLog
    {
        public long LogId { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; }
        public string ModuleKey { get; set; }
        public int? EntityId { get; set; }
        public string Description { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ── i18n ─────────────────────────────────────────────────
    public class Language
    {
        public int LanguageId { get; set; }
        public string CultureCode { get; set; }
        public string DisplayName { get; set; }
        public string NativeName { get; set; }
        public string FlagIconUrl { get; set; }
        public int SortOrder { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }

    public class Translation
    {
        public int TranslationId { get; set; }
        public string ResourceKey { get; set; }
        public int LanguageId { get; set; }
        public string Value { get; set; }
        public string ModuleKey { get; set; }
        public bool IsAutomatic { get; set; }
    }

    // ── Site Settings ─────────────────────────────────────────
    public class SiteSetting
    {
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
        public string SettingGroup { get; set; }
        public string DataType { get; set; }
        public bool IsEncrypted { get; set; }
        public string Description { get; set; }
    }

    public class PageMeta
    {
        public int MetaId { get; set; }
        public string PageKey { get; set; }
        public int LanguageId { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string OGTitle { get; set; }
        public string OGDescription { get; set; }
        public string OGImageUrl { get; set; }
        public string SchemaJson { get; set; }
    }

    // ── Home ─────────────────────────────────────────────────
    public class HeroSlide
    {
        public int SlideId { get; set; }
        public int LanguageId { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string BackgroundImageUrl { get; set; }
        public string ButtonText { get; set; }
        public string ButtonUrl { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }

    public class Announcement
    {
        public int AnnouncementId { get; set; }
        public int LanguageId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public byte Priority { get; set; }  // 1=Normal, 2=Important, 3=Urgent
        public DateTime PublishDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string LanguageName { get; set; }  // joined
        public string CreatedByName { get; set; }  // joined
        public string CreatedBy { get; set; } 
        public string PriorityLabel => Priority switch
        {
            3 => "Urgent",
            2 => "Important",
            _ => "Normal"
        };
        public string PriorityBadgeClass => Priority switch
        {
            3 => "badge bg-danger",
            2 => "badge bg-warning",
            _ => "badge bg-secondary"
        };
    }

    public class QuickLink
    {
        public int LinkId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string IconClass { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    // ── Mass Timing ───────────────────────────────────────────
    public class MassSchedule
    {
        public int ScheduleId { get; set; }
        public int LanguageId { get; set; }
        public byte DayOfWeek { get; set; }
        public string DayName { get; set; }  // joined
        public TimeSpan MassTime { get; set; }
        public string MassLanguage { get; set; }
        public string Venue { get; set; }
        public string Celebrant { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public string LanguageName { get; set; }

        public string MassTimeDisplay => DateTime.Today.Add(MassTime).ToString("hh:mm tt");
        public string CreatedBy { get; set; }
    }

    public class SpecialMass
    {
        public int SpecialMassId { get; set; }
        public int LanguageId { get; set; }
        public string Title { get; set; }
        public DateTime MassDate { get; set; }
        public TimeSpan MassTime { get; set; }
        public string Venue { get; set; }
        public string Description { get; set; }
        public bool IsRecurring { get; set; }
        public bool IsActive { get; set; }
    }

    // ── Prayer Requests ───────────────────────────────────────
    public class PrayerRequest
    {
        public int RequestId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PrayerCategory { get; set; }
        public string PrayerText { get; set; }
        public bool IsAnonymous { get; set; }
        public byte StatusId { get; set; }
        public string AdminNotes { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string ReviewedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IpAddress { get; set; }
        public string StatusLabel => StatusId switch
        {
            2 => "Approved",
            3 => "Rejected",
            _ => "Pending"
        };
        public string StatusBadgeClass => StatusId switch
        {
            2 => "badge bg-success",
            3 => "badge bg-danger",
            _ => "badge bg-warning"
        };

        public int LanguageId { get; set; }
    }

    // ── Testimonials ──────────────────────────────────────────
    public class Testimonial
    {
        public int TestimonialId { get; set; }
        public int LanguageId { get; set; }
        public string AuthorName { get; set; }
        public string AuthorLocation { get; set; }
        public string PhotoUrl { get; set; }
        public string TestimonialText { get; set; }
        public byte Rating { get; set; }
        public byte StatusId { get; set; }
        public bool FeaturedOnHome { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ── Daily Content ─────────────────────────────────────────
    public class FeastOfDay
    {
        public int FeastId { get; set; }
        public int LanguageId { get; set; }
        public DateTime FeastDate { get; set; }
        public string SaintName { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool IsAutomatic { get; set; }
        public string CreatedBy { get; set; }
    }

    public class ReadingOfDay
    {
        public int ReadingId { get; set; }
        public int LanguageId { get; set; }
        public DateTime ReadingDate { get; set; }
        public string FirstReading { get; set; }
        public string FirstReadingRef { get; set; }
        public string Psalm { get; set; }
        public string PsalmRef { get; set; }
        public string SecondReading { get; set; }
        public string SecondReadingRef { get; set; }
        public string Gospel { get; set; }
        public string GospelRef { get; set; }
        public string LiturgicalColor { get; set; }
        public string Source { get; set; }
    }

    public class ThoughtOfDay
    {
        public int ThoughtId { get; set; }
        public int LanguageId { get; set; }
        public DateTime ThoughtDate { get; set; }
        public string Quote { get; set; }
        public string Attribution { get; set; }
        public string BackgroundImageUrl { get; set; }
        public int CreatedBy { get; set; }
    }

    // ── Media: Gallery ────────────────────────────────────────
    public class GalleryCategory
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class GalleryAlbum
    {
        public int AlbumId { get; set; }
        public int LanguageId { get; set; }
        public int? CategoryId { get; set; }
        public string AlbumName { get; set; }
        public string Description { get; set; }
        public string CoverImageUrl { get; set; }
        public DateTime? EventDate { get; set; }
        public bool IsPublished { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CategoryName { get; set; }
        public int PhotoCount { get; set; }
        public string CreatedBy { get; set; }
    }

    public class GalleryPhoto
    {
        public int PhotoId { get; set; }
        public int AlbumId { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Caption { get; set; }
        public string AltText { get; set; }
        public int SortOrder { get; set; }
        public bool IsPublished { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int FileSizeKb { get; set; }
    }

    // ── Priests ───────────────────────────────────────────────
    public class Priest
    {
        public int PriestId { get; set; }
        public int LanguageId { get; set; }
        public string FullName { get; set; }
        public string Title { get; set; }
        public string PhotoUrl { get; set; }
        public string Bio { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public int SortOrder { get; set; }

        public string Tenure => StartDate.HasValue
            ? $"{StartDate:MMM yyyy} – {(EndDate.HasValue ? EndDate.Value.ToString("MMM yyyy") : "Present")}"
            : string.Empty;

        public int CreatedBy { get; set; }
    }

    // ── Room Booking ──────────────────────────────────────────
    public class Room
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public int Capacity { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal HourlyRate { get; set; }
        public string Amenities { get; set; }
        public bool IsActive { get; set; }
    }

    public class RoomBooking
    {
        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public string BookedByName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Purpose { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public byte StatusId { get; set; }
        public string AdminNotes { get; set; }
        public string BookingRef { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ReviewedByName { get; set; }

        public string StatusLabel => StatusId switch
        {
            2 => "Approved",
            3 => "Rejected",
            4 => "Cancelled",
            _ => "Pending"
        };
        public string StatusBadgeClass => StatusId switch
        {
            2 => "badge bg-success",
            3 => "badge bg-danger",
            4 => "badge bg-secondary",
            _ => "badge bg-warning"
        };
    }

    // ── Donation ──────────────────────────────────────────────
    public class DonationCategory
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }

    public class Donation
    {
        public long DonationId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string DonorName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentGateway { get; set; }
        public string GatewayOrderId { get; set; }
        public string GatewayPaymentId { get; set; }
        public byte StatusId { get; set; }
        public bool IsAnonymous { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public string IpAddress { get; set; }
        public string StatusLabel => StatusId switch
        {
            2 => "Success",
            3 => "Failed",
            _ => "Initiated"
        };
    }

    // ── Dashboard ─────────────────────────────────────────────
    public class DashboardStats
    {
        public int PendingPrayers { get; set; }
        public int PendingTestimonials { get; set; }
        public int PendingBookings { get; set; }
        public decimal DonationsToday { get; set; }
        public int DonationCountToday { get; set; }
        public int ActiveAdminUsers { get; set; }
    }
    // ── Songs ──────────────────────────────────────────────────
    public class SongCategory
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int LanguageId { get; set; }
        public string ColorHex { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class Song
    {
        public int SongId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int LanguageId { get; set; }
        public string Title { get; set; }
        public string Lyrics { get; set; }
        public string AudioFileUrl { get; set; }
        public int? Duration { get; set; }
        public int SortOrder { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
    }
}