using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using DreamAmazon.Interfaces;
using DreamAmazon.Models;
using Microsoft.Practices.ServiceLocation;

namespace DreamAmazon
{
    public class CaptchaState : CheckState
    {
        protected string PostUrl;
        protected string GuessField;

        private const int CountersLimit = 3;

        private int _captchaCounter;
        private int _notDbcModeCounter;
        private string _response;
        private SettingModel _setting;

        public CaptchaState(StateContext context) : base(context)
        {
            PostUrl = Globals.POST_URL;
            GuessField = "guess";
            _setting = ServiceLocator.Current.GetInstance<ISettingsService>().GetSettings();
        }

        public override void Init(string response)
        {
            _response = response;
        }

        public override void Handle(NetHelper nHelper)
        {
            var account = Context.CheckParams.Account;
            var proxyManager = Context.ProxyManager;

            if (_setting.IsDuoMode || _setting.IsDbcMode)
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

                    attributes.Add(GuessField, captchaResult.Value.Text);

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