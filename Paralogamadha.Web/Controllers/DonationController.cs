using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Paralogamadha.Core.Interfaces;
using Paralogamadha.Core.Models;

namespace Paralogamadha.Web.Controllers
{
    public class DonationController : BaseController
    {
        private readonly IEmailService _email;

        public DonationController(IUnitOfWork uow, ITranslationService t, ISeoService seo, IEmailService email)
            : base(uow, t, seo) => _email = email;

        public ActionResult Index()
        {
            SetPageMeta("donation");
            ViewBag.Categories = _uow.Donations.GetCategories();
            ViewBag.RazorpayKey = _uow.SiteSettings.GetValue("payment.razorpayKeyId");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult CreateOrder(Donation model)
        {
            try
            {
                var keyId = _uow.SiteSettings.GetValue("payment.razorpayKeyId");
                var keySecret = _uow.SiteSettings.GetValue("payment.razorpaySecret");

                if (string.IsNullOrEmpty(keyId) || string.IsNullOrEmpty(keySecret))
                    return JsonError("Payment gateway not configured. Please contact the parish office.");

                if (model.Amount < 10)
                    return JsonError("Minimum donation amount is ₹10.");

                // Create Razorpay order via REST
                var client = new System.Net.Http.HttpClient();
                var authBytes = Encoding.ASCII.GetBytes($"{keyId}:{keySecret}");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));

                var orderPayload = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    amount = (int)(model.Amount * 100), // paise
                    currency = "INR",
                    receipt = $"PBL-{DateTime.UtcNow:yyyyMMddHHmmss}"
                });

                var response = client.PostAsync("https://api.razorpay.com/v1/orders",
                    new System.Net.Http.StringContent(orderPayload, Encoding.UTF8, "application/json"))
                    .GetAwaiter().GetResult();

                if (!response.IsSuccessStatusCode)
                    return JsonError("Could not create payment order. Please try again.");

                var orderJson = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                dynamic order = Newtonsoft.Json.JsonConvert.DeserializeObject(orderJson);
                string orderId = order.id;

                // Save pending donation to DB
                model.PaymentGateway = "Razorpay";
                model.GatewayOrderId = orderId;
                model.Currency = "INR";
                model.IpAddress = ClientIp();
                var donationId = _uow.Donations.Insert(model);

                return JsonSuccess(new
                {
                    orderId,
                    donationId,
                    amountPaise = (int)(model.Amount * 100)
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Razorpay order creation failed: {ex}");
                return JsonError("Payment initiation failed. Please try again.");
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> Verify(long donationId, string razorpayPaymentId,
            string razorpayOrderId, string razorpaySignature)
        {
            try
            {
                var keySecret = _uow.SiteSettings.GetValue("payment.razorpaySecret");

                // Verify signature: HMAC-SHA256(orderId + "|" + paymentId, keySecret)
                var payload = $"{razorpayOrderId}|{razorpayPaymentId}";
                var keyBytes = Encoding.UTF8.GetBytes(keySecret);
                var msgBytes = Encoding.UTF8.GetBytes(payload);
                string computed;
                using (var hmac = new HMACSHA256(keyBytes))
                {
                    var hash = hmac.ComputeHash(msgBytes);
                    computed = BitConverter.ToString(hash).Replace("-", "").ToLower();
                }

                if (!string.Equals(computed, razorpaySignature, StringComparison.OrdinalIgnoreCase))
                {
                    _uow.Donations.UpdateStatus(donationId, 3, razorpayPaymentId, razorpaySignature);
                    return JsonError("Payment signature verification failed.");
                }

                // Mark success
                _uow.Donations.UpdateStatus(donationId, 2, razorpayPaymentId, razorpaySignature);

                // Send receipt
                var donation = _uow.Donations.GetById(donationId);
                if (donation != null)
                {
                    donation.CategoryName = _uow.Donations.GetCategories()
                        .FirstOrDefault(c => c.CategoryId == donation.CategoryId)?.CategoryName;
                    donation.PaymentDate = DateTime.UtcNow;
                    donation.GatewayPaymentId = razorpayPaymentId;
                    try { await _email.SendDonationReceiptAsync(donation); } catch { }
                }

                return JsonSuccess(new { transactionId = razorpayPaymentId }, "Payment successful!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Donation verify failed: {ex}");
                return JsonError("Verification error. Contact parish office.");
            }
        }
    }
}