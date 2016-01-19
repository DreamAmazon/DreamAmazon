namespace DreamAmazon
{
    public class RoboState : CaptchaState
    {
        public RoboState(StateContext context) : base(context)
        {
            PostUrl = @"https://www.amazon.com/errors/validateCaptcha";
            GuessField = "field-keywords";
        }
    }
}