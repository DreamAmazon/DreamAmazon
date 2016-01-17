using System.Threading.Tasks;

namespace DreamAmazon.Interfaces
{
    public interface ICaptchaService
    {
        double GetBalance();
        Result<CaptchaDecodeResult> DecodeCaptcha(byte[] image);
        Task<Result<CaptchaDecodeResult>> DecodeCaptchaAsync(byte[] image);
        Task<CaptchaLoginResult> LoginAsync(string user, string pass);
    }
}