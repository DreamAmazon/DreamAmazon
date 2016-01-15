using System;
using System.IO;
using DeathByCaptcha;

namespace DreamAmazon.Services
{
    public abstract class BaseCaptchaService
    {
        protected void DebugCaptcha(byte[] image, Captcha captchaResult)
        {
            var guid = Guid.NewGuid();

            var fileName = string.Format("{0}-{1}", guid, captchaResult.Text);

            File.WriteAllBytes(fileName + ".jpg", image);

            var captchaText = string.Format("Id={0}\nCorrect={1}\nText={2}\nSolved={3}\nUploaded={4}", captchaResult.Id, captchaResult.Correct,
                captchaResult.Text, captchaResult.Solved, captchaResult.Uploaded);

            File.WriteAllText(fileName + ".txt", captchaText);

        }
    }
}