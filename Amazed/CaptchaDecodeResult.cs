namespace DreamAmazon
{
    public class CaptchaDecodeResult
    {
        public string Text { get; }

        public CaptchaDecodeResult(string text)
        {
            Text = text;
        }
    }
}