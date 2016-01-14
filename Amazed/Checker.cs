using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DreamAmazon.Events;
using DreamAmazon.Interfaces;
using EventAggregatorNet;
using Microsoft.Practices.ServiceLocation;

namespace DreamAmazon
{
    public class CheckParams
    {
        public Account Account { get; }

        public CheckParams(Account account)
        {
            Contracts.Require(account != null);

            Account = account;
        }
    }

    public enum CheckResults
    {
        Good,
        Bad,
        Init
    }


    public class Checker
    {
        #region Fields

        private readonly ICaptchaService _captchaService;
        private readonly IProxyManager _proxyManager;
        private readonly IAccountManager _accountManager;
        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;

        #endregion

        #region Events

        public delegate void CheckDoneDelegate(CheckResults results, Account account);
        public event CheckDoneDelegate OnCheckCompleted;

        #endregion

        #region Properties

        public int AccountsLeft
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            get { return _accountManager.Count - AccountsChecked; }           
        }

        private int _accChecked;
        public int AccountsChecked
        {
            get { return _accChecked; }
        }

        private int _validAccounts;
        public int ValidAccounts
        {
            get { return _validAccounts; }
        }

        private int _badAccounts;
        public int BadAccounts
        {
            get { return _badAccounts; }
        }

        private int _threadCounter;

        public int ActiveThreads
        {
            get { return _threadCounter; }
            set { _threadCounter = value; }
        }

        #endregion

        public Checker(ICaptchaService captchaService, IProxyManager proxyManager, IAccountManager accountManager)
        {
            _captchaService = captchaService;
            _proxyManager = proxyManager;
            _accountManager = accountManager;
            _logger = ServiceLocator.Current.GetInstance<ILogger>();
            _eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
        }

        private void _context_OnCheckCompleted(StateContext context, CheckResults results, CheckParams checkParams)
        {
            if (results != CheckResults.Init)
            {
                Interlocked.Increment(ref _accChecked);
                if (results == CheckResults.Bad)
                {
                    Interlocked.Increment(ref _badAccounts);
                }
                else if (results == CheckResults.Good)
                {
                    Interlocked.Increment(ref _validAccounts);
                }
            }
            FireOnCheckCompleted(results, checkParams);
        }

        public async Task<bool> InitCoreAsync()
        {
            if (Properties.Settings.Default.Mode == 0 || Properties.Settings.Default.Mode == 1)
            {
                var loginResult = await _captchaService.LoginAsync(Properties.Settings.Default.DBCUser, Properties.Settings.Default.DBCPass);
                return !loginResult.IsFail;
            }

            return true;
        }

        public void Start(CancellationToken token)
        {
            Contracts.Require(token != null);

            FireOnCheckCompleted(CheckResults.Init, null);

            _threadCounter = 0;

            ResetCounters();

            ParallelOptions options = new ParallelOptions
            {
                CancellationToken = token,
                MaxDegreeOfParallelism = Properties.Settings.Default.Threads
            };

            var exceptions = new ConcurrentQueue<Exception>();

            Parallel.ForEach(_accountManager.Accounts, options, account =>
            {
                Interlocked.Increment(ref _threadCounter);
                
                try
                {
                    Check(new CheckParams(account), token);
                }
                catch (Exception exception)
                {
                    // Store the exception and continue with the loop.                    
                    exceptions.Enqueue(exception);
                    _logger.Error(exception);
                }

                Interlocked.Decrement(ref _threadCounter);
            });

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions);
        }

        public void ResetCounters()
        {
            _accChecked = 0;
            _validAccounts = 0;
            _badAccounts = 0;
        }

        private void Check(CheckParams checkParams, CancellationToken token)
        {
            Contracts.Require(checkParams != null);

            try
            {
                NetHelper nHelper = new NetHelper { UserAgent = UserAgentsManager.GetRandomUserAgent() };

                var context = new StateContext(_logger, _proxyManager, _captchaService);
                context.OnCheckCompleted += _context_OnCheckCompleted;
                context.Handle(checkParams, nHelper, token);
                context.OnCheckCompleted -= _context_OnCheckCompleted;
            }
            catch (Exception exception)
            {
                _logger.Debug("error while check");
                _logger.Error(exception);

                _accChecked++;
                _badAccounts++;

                FireOnCheckCompleted(CheckResults.Bad, checkParams);

                _eventAggregator.SendMessage(new InformUserMessage(new InformUserMessage.Message(exception.Message, InformUserMessage.MessageType.Error)));
            }
        }

        private void FireOnCheckCompleted(CheckResults results, CheckParams checkParams)
        {
            var evt = OnCheckCompleted;
            // ReSharper disable once UseNullPropagation
            if (evt != null)
            {
                if (checkParams == null)
                    evt(results, null);
                else
                    evt(results, checkParams.Account);
            }
        }
    }
}
