using System.Threading.Tasks;
using DeathByCaptcha;
using DreamAmazon.Events;
using DreamAmazon.Interfaces;
using EventAggregatorNet;
using Microsoft.Practices.ServiceLocation;

namespace DreamAmazon.Services
{
    public class TestCaptchaService : BaseCaptchaService, ICaptchaService
    {
        private readonly IEventAggregator _eventAggregator;

        public TestCaptchaService()
        {
            _eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
        }

        public double GetBalance()
        {
            return 999;
        }

        public Result<CaptchaDecodeResult> DecodeCaptcha(byte[] image)
        {
            string text = "123ab";
            var captchaResult = new CaptchaDecodeResult(text);
            DebugCaptcha(image, new Captcha { Text = captchaResult.Text });
            return Result.Ok(captchaResult);
        }

        public async Task<Result<CaptchaDecodeResult>> DecodeCaptchaAsync(byte[] image)
        {
            return await Task<Result<CaptchaDecodeResult>>.Factory.StartNew(() => DecodeCaptcha(image));
        }

        public async Task<CaptchaLoginResult> LoginAsync(string user, string pass)
        {
            return await Task<CaptchaLoginResult>.Factory.StartNew(() =>
            {
                _eventAggregator.SendMessage(new BalanceRetrievedMessage(GetBalance()));
                return new CaptchaLoginResult(true);
            });
        }
    }
}