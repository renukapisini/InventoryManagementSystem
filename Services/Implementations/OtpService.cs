using System.Security.Cryptography;
using System.Text;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services.Implementations
{
    public class OtpService : IOtpService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<OtpService> _logger;

        public OtpService(ApplicationDbContext context, IEmailService emailService, ILogger<OtpService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, string OtpCodeForDev)> GenerateAndSendOtpAsync(string email, string purpose = "Registration")
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return (false, "Email address is required.", string.Empty);
            }

            var cleanEmail = email.Trim().ToLower();

            // 60-second cooldown check
            if (!await CanResendOtpAsync(cleanEmail, purpose))
            {
                return (false, "Please wait at least 60 seconds before requesting a new OTP.", string.Empty);
            }

            // Invalidate existing unused active OTPs for this email/purpose
            var existingOtps = await _context.OtpVerifications
                .Where(o => o.Email == cleanEmail && o.Purpose == purpose && !o.IsUsed)
                .ToListAsync();

            foreach (var existing in existingOtps)
            {
                existing.IsUsed = true;
            }

            // Generate cryptographically secure 6-digit OTP
            var otpNum = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var otpHash = HashOtp(otpNum);

            var otpRecord = new OtpVerification
            {
                Email = cleanEmail,
                OtpHash = otpHash,
                ExpiryTime = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false,
                AttemptCount = 0,
                Purpose = purpose,
                CreatedDate = DateTime.UtcNow
            };

            _context.OtpVerifications.Add(otpRecord);
            await _context.SaveChangesAsync();

            // Email subject & body
            var subject = $"Inventory Management System - {purpose} Verification Code";
            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
                    <h2>Verification Code</h2>
                    <p>Your 6-digit OTP for <strong>{purpose}</strong> is:</p>
                    <div style='background-color: #f4f4f5; padding: 15px; border-radius: 8px; font-size: 24px; font-weight: bold; letter-spacing: 5px; text-align: center; color: #4f46e5;'>
                        {otpNum}
                    </div>
                    <p style='margin-top: 15px; font-size: 13px; color: #666;'>
                        This code will expire in 10 minutes. If you did not request this code, please ignore this email.
                    </p>
                </div>";

            await _emailService.SendEmailNotificationAsync(cleanEmail, subject, emailBody);
            _logger.LogInformation("OTP generated for {Email} ({Purpose})", cleanEmail, purpose);

            return (true, "OTP has been sent to your email address.", otpNum);
        }

        public async Task<(bool Success, string Message)> VerifyOtpAsync(string email, string otpCode, string purpose = "Registration")
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otpCode))
            {
                return (false, "Email and OTP code are required.");
            }

            var cleanEmail = email.Trim().ToLower();
            var cleanCode = otpCode.Trim();

            var otpRecord = await _context.OtpVerifications
                .Where(o => o.Email == cleanEmail && o.Purpose == purpose && !o.IsUsed)
                .OrderByDescending(o => o.CreatedDate)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
            {
                return (false, "No active OTP found. Please request a new verification code.");
            }

            if (DateTime.UtcNow > otpRecord.ExpiryTime)
            {
                otpRecord.IsUsed = true;
                await _context.SaveChangesAsync();
                return (false, "Your OTP code has expired. Please request a new verification code.");
            }

            if (otpRecord.AttemptCount >= 5)
            {
                otpRecord.IsUsed = true;
                await _context.SaveChangesAsync();
                return (false, "Maximum verification attempts exceeded. Please request a new OTP.");
            }

            otpRecord.AttemptCount++;

            var inputHash = HashOtp(cleanCode);
            if (otpRecord.OtpHash != inputHash)
            {
                await _context.SaveChangesAsync();
                var remainingAttempts = 5 - otpRecord.AttemptCount;
                return (false, $"Invalid OTP code. {remainingAttempts} attempt(s) remaining.");
            }

            // OTP verified successfully
            otpRecord.IsUsed = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("OTP verified successfully for {Email} ({Purpose})", cleanEmail, purpose);
            return (true, "OTP verified successfully.");
        }

        public async Task<bool> CanResendOtpAsync(string email, string purpose = "Registration")
        {
            var cleanEmail = email.Trim().ToLower();
            var latest = await _context.OtpVerifications
                .Where(o => o.Email == cleanEmail && o.Purpose == purpose)
                .OrderByDescending(o => o.CreatedDate)
                .FirstOrDefaultAsync();

            if (latest == null) return true;

            // 60-second cooldown
            return (DateTime.UtcNow - latest.CreatedDate).TotalSeconds >= 60;
        }

        private static string HashOtp(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes($"ims_salt_{input}"));
            return Convert.ToBase64String(bytes);
        }
    }
}
