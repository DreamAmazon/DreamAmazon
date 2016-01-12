using System.Threading.Tasks;

namespace DreamAmazon.Interfaces
{
    public interface ICaptchaService
    {
        double GetBalance();
        Task<CaptchaDecodeResult> DecodeCaptchaAsync(byte[] image);
        Task<CaptchaLoginResult> LoginAsync(string user, string pass);
    }
}