// ============================================================
//  Paralogamadha.Services / Services.cs
//  AuthService, TranslationService, FileUploadService, EmailService
// ============================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Mail;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Paralogamadha.Core.Interfaces;
using Paralogamadha.Core.Models;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Drawing.Image;

namespace Paralogamadha.Services
{
    // ── Auth Service ──────────────────────────────────────────
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;
        private const int SaltSize    = 32;
        private const int HashSize    = 64;
        private const int Iterations  = 100_000;

        public AuthService(IUnitOfWork uow) => _uow = uow;

        public (bool Success, string Error, ApplicationUser User) Login(
            string username, string password, string ip)
        {
            var user = _uow.Users.GetByUsername(username);

            if (user == null)
                return (false, "Invalid username or password.", null);

            if (!user.IsActive)
                return (false, "This account has been deactivated.", null);

            if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
                return (false, $"Account locked. Try again after {user.LockedUntil.Value:hh:mm tt} UTC.", null);

            if (!VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            {
                _uow.Users.UpdateLoginFailed(user.UserId);
                _uow.Users.InsertAuditLog(new AuditLog
                {
                    UserId = user.UserId, Action = "LOGIN_FAILED",
                    IpAddress = ip, Description = $"Failed login attempt for {username}"
                });
                return (false, "Invalid username or password.", null);
            }

            _uow.Users.UpdateLoginSuccess(user.UserId);
            _uow.Users.InsertAuditLog(new AuditLog
            {
                UserId = user.UserId, Action = "LOGIN_SUCCESS",
                IpAddress = ip, Description = $"Successful login"
            });

            return (true, null, user);
        }

        public string HashPassword(string password, out string salt)
        {
            var saltBytes = new byte[SaltSize];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(saltBytes);

            salt = Convert.ToBase64String(saltBytes);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256))
            {
                return Convert.ToBase64String(pbkdf2.GetBytes(HashSize));
            }
        }

        public bool VerifyPassword(string password, string hash, string saltBase64)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(saltBase64)) return false;

                // Clean the string: remove common whitespace/newlines that break Base64 decoding
                string cleanSalt = saltBase64.Trim().Replace(" ", "").Replace("\n", "").Replace("\r", "");

                // Ensure the string length is a multiple of 4 (valid Base64 padding)
                if (cleanSalt.Length % 4 != 0) return false;

                var saltBytes = Convert.FromBase64String(cleanSalt);
                using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
                var computedHash = Convert.ToBase64String(pbkdf2.GetBytes(HashSize));
                return SlowEquals(hash, computedHash);
            }
            catch { return false; }
        }
        

        // Constant-time comparison to prevent timing attacks
        private static bool SlowEquals(string a, string b)
        {
            var diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= (uint)(a[i] ^ b[i]);
            return diff == 0;
        }
    }

    // ── Translation Service ───────────────────────────────────
    public class TranslationService : ITranslationService
    {
        private readonly IUnitOfWork    _uow;
        private static readonly MemoryCache _cache = MemoryCache.Default;
        private const int CacheMinutes = 30;

        public TranslationService(IUnitOfWork uow) => _uow = uow;

        public string Get(string key, string languageCode, string fallback = "")
        {
            var lang    = _uow.Languages.GetByCode(languageCode);
            if (lang == null) return fallback;

            var cacheKey = $"trans_{lang.LanguageId}_{key}";
            if (_cache[cacheKey] is string cached) return cached;

            var value = _uow.Translations.Get(key, lang.LanguageId) ?? fallback;
            _cache.Set(cacheKey, value, DateTimeOffset.Now.AddMinutes(CacheMinutes));
            return value;
        }

        public Dictionary<string, string> GetModule(string moduleKey, string languageCode)
        {
            var lang    = _uow.Languages.GetByCode(languageCode);
            if (lang == null) return new Dictionary<string, string>();

            var cacheKey = $"trans_mod_{lang.LanguageId}_{moduleKey}";
            if (_cache[cacheKey] is Dictionary<string, string> cached) return cached;

            var dict = _uow.Translations.GetByModule(moduleKey, lang.LanguageId);
            _cache.Set(cacheKey, dict, DateTimeOffset.Now.AddMinutes(CacheMinutes));
            return dict;
        }

        public void Set(string key, string languageCode, string value, string moduleKey)
        {
            var lang = _uow.Languages.GetByCode(languageCode);
            if (lang == null) return;

            _uow.Translations.Upsert(key, lang.LanguageId, value, moduleKey);

            // Invalidate cache
            var cacheKey = $"trans_{lang.LanguageId}_{key}";
            _cache.Remove(cacheKey);
            _cache.Remove($"trans_mod_{lang.LanguageId}_{moduleKey}");
        }
    }

    // ── File Upload Service ───────────────────────────────────
    public class FileUploadService : IFileUploadService
    {
        private readonly string _uploadRoot;
        private static readonly string[] AllowedImageMime  = { "image/jpeg", "image/png", "image/webp" };
        private static readonly string[] AllowedImageExt   = { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] AllowedAudioExt   = { ".mp3", ".ogg", ".wav" };
        private const int MaxImageSizeMb = 5;
        private const int MaxAudioSizeMb = 50;
        private const int ThumbWidth     = 400;
        private const int ThumbHeight    = 300;

        public FileUploadService(string uploadRootPath)
        {
            _uploadRoot = uploadRootPath;
        }

        public UploadResult UploadImage(HttpPostedFileBase file, string folder)
        {
            try
            {
                if (file == null || file.ContentLength == 0)
                    return Fail("No file provided.");

                if (file.ContentLength > MaxImageSizeMb * 1024 * 1024)
                    return Fail($"File size exceeds {MaxImageSizeMb}MB limit.");

                var ext = Path.GetExtension(file.FileName).ToLower();
                if (!Array.Exists(AllowedImageExt, e => e == ext))
                    return Fail("Invalid file type. Only JPG, PNG, WEBP allowed.");

                // Validate magic bytes
                if (!IsValidImageMagic(file.InputStream))
                    return Fail("File content does not match image type.");

                file.InputStream.Seek(0, SeekOrigin.Begin);

                var folderPath = Path.Combine(_uploadRoot, folder);
                Directory.CreateDirectory(folderPath);

                var fileName  = $"{Guid.NewGuid():N}{ext}";
                var thumbName = $"{Path.GetFileNameWithoutExtension(fileName)}_thumb{ext}";
                var fullPath  = Path.Combine(folderPath, fileName);
                var thumbPath = Path.Combine(folderPath, thumbName);

                // Re-encode to strip EXIF/embedded content
                using var original = Image.FromStream(file.InputStream);
                SaveCleanImage(original, fullPath, ext);
                GenerateThumbnail(original, thumbPath, ext);

                return new UploadResult
                {
                    Success       = true,
                    FilePath      = $"/uploads/{folder}/{fileName}",
                    ThumbnailPath = $"/uploads/{folder}/{thumbName}",
                    FileName      = fileName,
                    WidthPx       = original.Width,
                    HeightPx      = original.Height,
                    FileSizeKb    = (int)(new FileInfo(fullPath).Length / 1024)
                };
            }
            catch (Exception ex)
            {
                return Fail($"Upload failed: {ex.Message}");
            }
        }

        public UploadResult UploadAudio(HttpPostedFileBase file, string folder)
        {
            try
            {
                if (file == null || file.ContentLength == 0)
                    return Fail("No file provided.");

                if (file.ContentLength > MaxAudioSizeMb * 1024 * 1024)
                    return Fail($"Audio file exceeds {MaxAudioSizeMb}MB limit.");

                var ext = Path.GetExtension(file.FileName).ToLower();
                if (!Array.Exists(AllowedAudioExt, e => e == ext))
                    return Fail("Invalid file type. Only MP3, OGG, WAV allowed.");

                var folderPath = Path.Combine(_uploadRoot, folder);
                Directory.CreateDirectory(folderPath);

                var fileName = $"{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(folderPath, fileName);
                file.SaveAs(fullPath);

                return new UploadResult
                {
                    Success    = true,
                    FilePath   = $"/uploads/{folder}/{fileName}",
                    FileName   = fileName,
                    FileSizeKb = (int)(new FileInfo(fullPath).Length / 1024)
                };
            }
            catch (Exception ex)
            {
                return Fail($"Audio upload failed: {ex.Message}");
            }
        }

        public bool Delete(string relativePath)
        {
            try
            {
                // Security: only delete within upload root
                var safeRelative = Path.GetFileName(relativePath);
                if (string.IsNullOrEmpty(safeRelative)) return false;

                var absolutePath = Path.Combine(_uploadRoot, relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (!absolutePath.StartsWith(_uploadRoot, StringComparison.OrdinalIgnoreCase))
                    return false;  // Path traversal attempt

                if (File.Exists(absolutePath))
                    File.Delete(absolutePath);
                return true;
            }
            catch { return false; }
        }

        private static bool IsValidImageMagic(Stream stream)
        {
            var header = new byte[4];
            stream.Read(header, 0, 4);
            stream.Seek(0, SeekOrigin.Begin);

            // JPEG: FF D8 FF
            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return true;
            // PNG: 89 50 4E 47
            if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) return true;
            // WEBP: 52 49 46 46 (RIFF)
            if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46) return true;

            return false;
        }

        private static void SaveCleanImage(System.Drawing.Image img, string path, string ext)
        {
            var codec = GetCodec(ext);
            using var ms = new MemoryStream();
            img.Save(ms, codec, null);
            File.WriteAllBytes(path, ms.ToArray());
        }

        private static void GenerateThumbnail(System.Drawing.Image original, string thumbPath, string ext)
        {
            using var thumb = original.GetThumbnailImage(ThumbWidth, ThumbHeight, () => false, IntPtr.Zero);
            var codec = GetCodec(ext);
            thumb.Save(thumbPath, codec, null);
        }

        private static ImageCodecInfo GetCodec(string ext)
        {
            string mime;

            switch (ext)
            {
                case ".png":
                    mime = "image/png";
                    break;
                case ".webp":
                    mime = "image/webp";
                    break;
                default:
                    mime = "image/jpeg";
                    break;
            }
            foreach (var codec in ImageCodecInfo.GetImageEncoders())
                if (codec.MimeType == mime) return codec;
            return ImageCodecInfo.GetImageEncoders()[1]; // fallback JPEG
        }

        private static UploadResult Fail(string error) =>
            new UploadResult { Success = false, Error = error };
    }

    // ── Email Service ─────────────────────────────────────────
    public class EmailService : IEmailService
    {
        private readonly string _host;
        private readonly int    _port;
        private readonly string _user;
        private readonly string _password;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(string host, int port, string user, string password, string fromEmail, string fromName)
        {
            _host      = host;
            _port      = port;
            _user      = user;
            _password  = password;
            _fromEmail = fromEmail;
            _fromName  = fromName;
        }

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            using var client = new SmtpClient(_host, _port)
            {
                EnableSsl   = true,
                Credentials = new System.Net.NetworkCredential(_user, _password)
            };
            using var mail = new MailMessage
            {
                From       = new MailAddress(_fromEmail, _fromName),
                Subject    = subject,
                Body       = htmlBody,
                IsBodyHtml = true
            };
            mail.To.Add(to);
            await client.SendMailAsync(mail);
        }

        public async Task SendBookingConfirmationAsync(RoomBooking booking)
        {
            var body = $@"
<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto'>
  <div style='background:#1B2A5B;padding:20px;text-align:center'>
    <h1 style='color:#C8922A;margin:0'>Paralogamadha Basilica</h1>
    <p style='color:#fff;margin:5px 0'>Room Booking {(booking.StatusId == 2 ? "Confirmed" : "Received")}</p>
  </div>
  <div style='padding:30px;background:#fff'>
    <p>Dear <strong>{HtmlEncode(booking.BookedByName)}</strong>,</p>
    <p>Your room booking has been <strong>{booking.StatusLabel}</strong>.</p>
    <table style='width:100%;border-collapse:collapse'>
      <tr><td style='padding:8px;border-bottom:1px solid #eee'><strong>Booking Ref</strong></td><td>{HtmlEncode(booking.BookingRef)}</td></tr>
      <tr><td style='padding:8px;border-bottom:1px solid #eee'><strong>Room</strong></td><td>{HtmlEncode(booking.RoomName)}</td></tr>
      <tr><td style='padding:8px;border-bottom:1px solid #eee'><strong>Date & Time</strong></td><td>{booking.StartDateTime:dddd, MMM dd yyyy} {booking.StartDateTime:hh:mm tt} – {booking.EndDateTime:hh:mm tt}</td></tr>
      <tr><td style='padding:8px'><strong>Purpose</strong></td><td>{HtmlEncode(booking.Purpose)}</td></tr>
    </table>
    {(string.IsNullOrEmpty(booking.AdminNotes) ? "" : $"<p><strong>Admin Note:</strong> {HtmlEncode(booking.AdminNotes)}</p>")}
  </div>
  <div style='background:#f5f5f5;padding:15px;text-align:center;font-size:12px;color:#666'>
    Paralogamadha Basilica of Our Lady of Assumption, Kamanayakkanpatti
  </div>
</div>";
            await SendAsync(booking.Email, $"Room Booking {booking.StatusLabel} – {booking.BookingRef}", body);
        }

        public async Task SendPrayerAcknowledgementAsync(PrayerRequest prayer)
        {
            if (string.IsNullOrEmpty(prayer.Email)) return;

            var body = $@"
<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto'>
  <div style='background:#1B2A5B;padding:20px;text-align:center'>
    <h1 style='color:#C8922A;margin:0'>Paralogamadha Basilica</h1>
  </div>
  <div style='padding:30px;background:#fff'>
    <p>Dear <strong>{HtmlEncode(prayer.FullName)}</strong>,</p>
    <p>We have received your prayer request and our community will pray for your intention.</p>
    <blockquote style='border-left:4px solid #C8922A;margin:20px 0;padding:10px 20px;background:#FFF8EE;font-style:italic'>
      {HtmlEncode(prayer.PrayerText)}
    </blockquote>
    <p>May Our Lady of Assumption intercede for your needs. God bless you.</p>
  </div>
  <div style='background:#f5f5f5;padding:15px;text-align:center;font-size:12px;color:#666'>
    Paralogamadha Basilica of Our Lady of Assumption, Kamanayakkanpatti
  </div>
</div>";
            await SendAsync(prayer.Email, "Prayer Request Received – Paralogamadha Basilica", body);
        }

        public async Task SendDonationReceiptAsync(Donation donation)
        {
            if (string.IsNullOrEmpty(donation.Email)) return;

            var body = $@"
<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto'>
  <div style='background:#1B2A5B;padding:20px;text-align:center'>
    <h1 style='color:#C8922A;margin:0'>Paralogamadha Basilica</h1>
    <p style='color:#fff;margin:5px 0'>Donation Receipt</p>
  </div>
  <div style='padding:30px;background:#fff'>
    <p>Dear <strong>{HtmlEncode(donation.DonorName)}</strong>,</p>
    <p>Thank you for your generous donation. May God bless you abundantly.</p>
    <table style='width:100%;border-collapse:collapse'>
      <tr style='background:#1B2A5B;color:#fff'><th style='padding:10px'>Description</th><th style='padding:10px'>Details</th></tr>
      <tr><td style='padding:8px;border-bottom:1px solid #eee'>Transaction ID</td><td>{HtmlEncode(donation.GatewayPaymentId)}</td></tr>
      <tr><td style='padding:8px;border-bottom:1px solid #eee'>Category</td><td>{HtmlEncode(donation.CategoryName)}</td></tr>
      <tr><td style='padding:8px;border-bottom:1px solid #eee'>Amount</td><td><strong>₹{donation.Amount:N2}</strong></td></tr>
      <tr><td style='padding:8px'>Date</td><td>{donation.PaymentDate:dddd, MMM dd yyyy}</td></tr>
    </table>
  </div>
  <div style='background:#f5f5f5;padding:15px;text-align:center;font-size:12px;color:#666'>
    This is an auto-generated receipt. Keep it for your records.<br/>
    Paralogamadha Basilica of Our Lady of Assumption, Kamanayakkanpatti
  </div>
</div>";
            await SendAsync(donation.Email, $"Donation Receipt – ₹{donation.Amount:N2} – Paralogamadha Basilica", body);
        }

        private static string HtmlEncode(string input) =>
            System.Web.HttpUtility.HtmlEncode(input ?? string.Empty);
    }

    // ── SEO Service ───────────────────────────────────────────
    public class SeoService : ISeoService
    {
        private readonly IUnitOfWork _uow;
        private static readonly MemoryCache _cache = MemoryCache.Default;

        public SeoService(IUnitOfWork uow) => _uow = uow;

        public PageMeta GetMeta(string pageKey, string languageCode)
        {
            var lang    = _uow.Languages.GetByCode(languageCode) ?? _uow.Languages.GetDefault();
            var cacheKey = $"meta_{pageKey}_{lang.LanguageId}";

            if (_cache[cacheKey] is PageMeta cached) return cached;

            var meta = _uow.PageMeta.GetByKey(pageKey, lang.LanguageId);
            if (meta != null)
                _cache.Set(cacheKey, meta, DateTimeOffset.Now.AddMinutes(60));

            return meta;
        }
    }
}
