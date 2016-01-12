using System;
using System.IO;
using System.Threading.Tasks;
using DeathByCaptcha;
using DreamAmazon.Events;
using DreamAmazon.Interfaces;
using EventAggregatorNet;
using Microsoft.Practices.ServiceLocation;

namespace DreamAmazon.Services
{
    public class DeathByCaptchaService : ICaptchaService
    {
        private readonly IEventAggregator _eventAggregator;
        private Client _dbcClient;
        private bool _isDebugMode;

        public DeathByCaptchaService(bool debug)
        {
            _isDebugMode = debug;
            _eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
        }

        public double GetBalance()
        {
            var balance = Math.Round(_dbcClient.Balance/100.0, 2);
            return balance;
        }

        public async Task<CaptchaDecodeResult> DecodeCaptchaAsync(byte[] image)
        {
            if (_dbcClient == null)
                throw new ApplicationException("make login first");

            var task = Task<CaptchaDecodeResult>.Factory.StartNew(() =>
            {
                var captchaResult = _dbcClient.Decode(image, (int) TimeSpan.FromMinutes(2).TotalSeconds);

                if (captchaResult != null && captchaResult.Solved && captchaResult.Correct)
                {
                    if (_isDebugMode)
                        DebugCaptcha(image, captchaResult);
                    return new CaptchaDecodeResult(captchaResult.Text);
                }

                return null;
            });
            return await task;
        }

        private void DebugCaptcha(byte[] image, Captcha captchaResult)
        {
            var guid = Guid.NewGuid();

            var fileName = string.Format("{0}-{1}", guid, captchaResult.Text);

            File.WriteAllBytes(fileName + ".jpg", image);

            var captchaText = string.Format("Id={0}\nCorrect={1}\nText={2}\nSolved={3}\nUploaded={4}", captchaResult.Id, captchaResult.Correct,
                captchaResult.Text, captchaResult.Solved, captchaResult.Uploaded);

            File.WriteAllText(fileName + ".txt", captchaText);

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