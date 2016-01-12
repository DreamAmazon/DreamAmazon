namespace DreamAmazon
{
    public class CaptchaLoginResult
    {
        public CaptchaLoginResult(bool success)
        {
            IsFail = !success;
        }

        public bool IsFail { get; }
    }
}