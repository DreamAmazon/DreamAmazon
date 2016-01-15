using System;
using System.Threading;
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
            return 1000;
        }

        public async Task<CaptchaDecodeResult> DecodeCaptchaAsync(byte[] image)
        {
            return await Task<CaptchaDecodeResult>.Factory.StartNew(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                var captchaResult = new CaptchaDecodeResult("123ab");
                DebugCaptcha(image, new Captcha {Text = captchaResult.Text});
                return captchaResult;
            });
        }

        public async Task<CaptchaLoginResult> LoginAsync(string user, string pass)
        {
            return await Task<CaptchaLoginResult>.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                _eventAggregator.SendMessage(new BalanceRetrievedMessage(GetBalance()));
                Thread.Sleep(1000);
                return new CaptchaLoginResult(true);
            });
        }
    }
}