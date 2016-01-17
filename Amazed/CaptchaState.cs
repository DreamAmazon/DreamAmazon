using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

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

    public class CaptchaState : CheckState
    {
        protected string PostUrl;
        protected string GuessField;

        private readonly Regex _attributesRegex = new Regex(Globals.REGEX, RegexOptions.Multiline | RegexOptions.IgnoreCase);

        private const int CountersLimit = 3;

        private int _captchaCounter;
        private int _notDbcModeCounter;
        private string _response;

        public CaptchaState(StateContext context) : base(context)
        {
            PostUrl = Globals.POST_URL;
            GuessField = "guess";
        }

        public override void Init(string response)
        {
            _response = response;
        }

        protected virtual KeyValuePair<string, string> GetGuessAttribute(string guessText)
        {
            return new KeyValuePair<string, string>("guess", guessText);
        }

        public override void Handle(NetHelper nHelper)
        {
            var account = Context.CheckParams.Account;
            var proxyManager = Context.ProxyManager;

            if (Properties.Settings.Default.Mode == 0 || Properties.Settings.Default.Mode == 1)
            {
                var captchaUrlResult = GetCaptchaUrl(_response);

                if (Context.IsError(captchaUrlResult))
                {
                    Context.SetFinishState(CheckResults.Bad);
                    return;
                }

                byte[] captchaBytes;

                using (WebClient wc = new WebClient())
                    captchaBytes = wc.DownloadData(captchaUrlResult.Value);

                var captchaResult = Context.CaptchaService.DecodeCaptcha(captchaBytes);

                if (captchaResult.Success)
                {
                    var metadata = Context.MetadataFinder.QueryMetadata(Context.CheckParams.Account);

                    Contracts.Require(metadata != null);

                    var attributes = StateContext.ParseAccountAttributes(_response, account, metadata);

                    //todo: not guess for robo
                    attributes.Add(GuessField, captchaResult.Value.Text);

                    //todo: 
                    var postResponse = nHelper.POST(PostUrl, attributes);

                    if (Context.IsError(postResponse))
                    {
                        Context.SetFinishState(CheckResults.Bad);
                        return;
                    }

                    // limit captcha attemts
                    if (_captchaCounter >= CountersLimit)
                    {
                        Context.Logger.Debug("captcha decoded, counter reached, finish state object:" + account.Email);
                        Context.SetFinishState(CheckResults.Bad);
                        return;
                    }

                    _captchaCounter++;
                    Context.Logger.Debug("captcha decoded, repeat current state object:" + account.Email);

                    Context.SetPreviousState(postResponse.Value);
                    return;
                }
                // captcha not recognized, so need to request new one
                Context.Logger.Debug("captcha not recognized, set restart state object:" + account.Email);
                Context.SetLoginState();
                return;
            }

            if (_notDbcModeCounter >= CountersLimit)
            {
                Context.Logger.Debug("not dbc mode, counter reached, finish state object:" + account.Email);
                Context.SetFinishState(CheckResults.Bad);
                return;
            }

            proxyManager.RemoveProxy(nHelper.Proxy);
            Context.SetLoginState();

            _notDbcModeCounter++;
            Context.Logger.Debug("not dbc mode, restart state object:" + account.Email);

            Context.SetPreviousState();
        }

        private static Result<string> GetCaptchaUrl(string response)
        {
            foreach (var sRegex in GetCaptchaUrlRegex())
            {
                if (!Regex.IsMatch(response, sRegex))
                    continue;

                //var matches = Regex.Matches(response, sRegex);
                //if (matches.Count > 1)
                //{}

                var match = Regex.Match(response, sRegex);
                return Result.Ok(match.Value.Trim('\"'));
            }
            return Result.Fail<string>("can't find captcha image");
        }

        private static IEnumerable<string> GetCaptchaUrlRegex()
        {
            yield return Globals.CaptchaUrlRegex;
        }
    }
}