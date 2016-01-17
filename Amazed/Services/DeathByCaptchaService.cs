using System;
using System.Threading.Tasks;
using DeathByCaptcha;
using DreamAmazon.Events;
using DreamAmazon.Interfaces;
using EventAggregatorNet;
using Microsoft.Practices.ServiceLocation;

namespace DreamAmazon.Services
{
    public class DeathByCaptchaService : BaseCaptchaService, ICaptchaService
    {
        private readonly IEventAggregator _eventAggregator;
        private Client _dbcClient;
        private readonly bool _isDebugMode;

        public DeathByCaptchaService(bool debug)
        {
            _isDebugMode = debug;
            _eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
        }

        public double GetBalance()
        {
            var rawBal = _dbcClient.GetBalance();
            var balance = Math.Round(rawBal / 100.0, 2);
            return balance;
        }

        public Result<CaptchaDecodeResult> DecodeCaptcha(byte[] image)
        {
            var captchaResult = _dbcClient.Decode(image, (int)TimeSpan.FromMinutes(2).TotalSeconds);

            if (captchaResult != null && captchaResult.Solved && captchaResult.Correct)
            {
                if (_isDebugMode)
                    DebugCaptcha(image, captchaResult);
                return Result.Ok(new CaptchaDecodeResult(captchaResult.Text));
            }

            return Result.Fail<CaptchaDecodeResult>("captcha does not recognized");
        }

        public async Task<Result<CaptchaDecodeResult>> DecodeCaptchaAsync(byte[] image)
        {
            Contracts.Require(_dbcClient != null);

            var task = Task<Result<CaptchaDecodeResult>>.Factory.StartNew(() => DecodeCaptcha(image));
            return await task;
        }

        public async Task<CaptchaLoginResult> LoginAsync(string dbcUser, string dbcPass)
        {
            var task = Task<CaptchaLoginResult>.Factory.StartNew(() =>
            {
                _dbcClient = new SocketClient(dbcUser, dbcPass) {Verbose = true};

                try
                {
                    var user = _dbcClient.User;

                    var balance = GetBalance();

                    _eventAggregator.SendMessage(new BalanceRetrievedMessage(balance));

                    if (user.LoggedIn && !user.Banned)
                    {
                        return new CaptchaLoginResult(true);
                    }
                    _dbcClient = null;
                    return new CaptchaLoginResult(false);
                }
                catch (System.Exception)
                {
                    _dbcClient = null;
                    return new CaptchaLoginResult(false);
                }
            });

            return await task;
        }
    }
}