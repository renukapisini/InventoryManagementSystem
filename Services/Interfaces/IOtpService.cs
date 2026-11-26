namespace InventoryManagementSystem.Services.Interfaces
{
    public interface IOtpService
    {
        Task<(bool Success, string Message, string OtpCodeForDev)> GenerateAndSendOtpAsync(string email, string purpose = "Registration");
        Task<(bool Success, string Message)> VerifyOtpAsync(string email, string otpCode, string purpose = "Registration");
        Task<bool> CanResendOtpAsync(string email, string purpose = "Registration");
    }
}
