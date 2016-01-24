using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DreamAmazon.Events;
using DreamAmazon.Interfaces;
using DreamAmazon.Models;
using EventAggregatorNet;
using Microsoft.Practices.ServiceLocation;

namespace DreamAmazon.Presenters
{
    public class MainViewPresenter : BasePresenter, IListener<BalanceRetrievedMessage>, IListener<SettingChangedMessage>, IListener<ProxiesStatisticMessage>
    {
        private readonly Checker _accountsChecker;
        private readonly ICaptchaService _captchaService;
        private readonly IProxyManager _proxyManager;
        private readonly IAccountManager _accountManager;
        private readonly ILogger _logger;

        private CancellationTokenSource _cancellationTokenSource;
        private readonly ISettingsService _settingsService;
        private readonly SettingModel _setting;

        private readonly IMainView _view;

        public MainViewPresenter(IMainView view)
        {
            Contracts.Require(view != null);

            _view = view;

            _view.StartCheckingRequested += View_StartCheckingRequested;
            _view.StopCheckingRequested += View_StopCheckingRequested;
            _view.DisplaySettingsRequested += View_DisplaySettingsRequested;
            _view.AboutRequested += View_AboutRequested;
            _view.ShowProxiesRequested += View_ShowProxiesRequested;
            _view.LoadAccountsRequested += View_LoadAccountsRequested;
            _view.CopyToClipboardRequested += View_CopyToClipboardRequested;
            _view.ExportToFileRequested += View_ExportToFileRequested;

            var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            eventAggregator.AddListener(this, true);

            _logger = ServiceLocator.Current.GetInstance<ILogger>();
            _captchaService = ServiceLocator.Current.GetInstance<ICaptchaService>();
            _proxyManager = ServiceLocator.Current.GetInstance<IProxyManager>();
            _accountManager = ServiceLocator.Current.GetInstance<IAccountManager>();
            _settingsService = ServiceLocator.Current.GetInstance<ISettingsService>();
            _setting = _settingsService.GetSettings();

            _accountsChecker = new Checker(_captchaService, _proxyManager, _accountManager);
            _accountsChecker.OnCheckCompleted += AccountsCheckerOnCheckCompleted;
        }

        private void View_ExportToFileRequested(bool detailed)
        {
            string output = string.Empty;
            var style = detailed ? _setting.CleanOutput : _setting.ShortOutput;

            SelectFileResult selectedFile = _view.GetUserFileToSave(new SelectFileOptions
            {
                Title = "Where do you want to export your accounts ?",
                FileName =
                    DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString().Replace(":", "h ") +
                    " Accounts.txt",
                Filter = "Text Files (*.txt)|*.txt"
            });

            if (selectedFile == SelectFileResult.Empty)
                return;

            foreach (Account account in _view.GetAccounts())
            {
                string data = AccountToString(account, style);
                output += data + "\r\n";

                if (detailed) output += "\r\n";
            }

            File.WriteAllText(selectedFile.FileName, output);
        }

        private void View_CopyToClipboardRequested(bool detailed)
        {
            string output = string.Empty;
            var style = detailed ? _setting.CleanOutput : _setting.ShortOutput;

            foreach (var account in _view.GetSelectedAccounts())
            {
                string data = AccountToString(account, style);
                output += data + "\r\n";

                if (detailed) output += "\r\n";
            }
            Clipboard.SetText(output);
        }

        private string AccountToString(Account account, string format)
        {
            string data = format;
            data = data.Replace("{Email}", account.Email);
            data = data.Replace("{Password}", account.Password);
            data = data.Replace("{Balance}", account.GiftCardBalance);
            data = data.Replace("{Order Quantity}", account.Orders);
            data = data.Replace("{Zip}", account.ZipCode);
            data = data.Replace("{Phone}", account.Phone);
            return data;
        }

        private void View_LoadAccountsRequested()
        {
            LoadAccounts();
            _view.ShowMessage("Idle", MessageType.Status);
        }

        private void View_ShowProxiesRequested()
        {
            var presenter = new ProxiesViewPresenter(new frmProxies());
            presenter.Start();
        }

        void AccountsCheckerOnCheckCompleted(CheckResults results, Account account)
        {
            ShowResult(results, account);
        }

        private void ShowResult(CheckResults results, Account account)
        {
            if (results == CheckResults.Good)
            {
                _view.DisplayAccount(account);
                _view.EnableExportRequest(true);
                var info = AccountToString(account, _setting.ShortOutput);
                _logger.Info(info);
            }

            UpdateUIInfos();
        }

        private void UpdateUIInfos()
        {
            _view.DisplayAccountsStatistic(GetAccountStatistic());
        }

        private AccountsStatistic GetAccountStatistic()
        {
            return new AccountsStatistic
            {
                Total = _accountManager.Count,
                Checked = _accountsChecker.AccountsChecked,
                Valid = _accountsChecker.ValidAccounts,
                Bad = _accountsChecker.BadAccounts,
                Threads = _accountsChecker.ActiveThreads
            };
        }

        private void ProcessStarted(object sender, DoWorkEventArgs e)
        {
            _accountsChecker.Start(_cancellationTokenSource.Token);
        }

        private void ProcessCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                _view.ShowMessage("Canceled", MessageType.Status);
            }
            else
            {
                if (e.Error is OperationCanceledException)
                    _view.ShowMessage("Canceled", MessageType.Status);
                else
                    _view.ShowMessage("Completed", MessageType.Status);
            }

            _view.EnableStopRequest(false);
            _view.EnableStartRequest(true);
            _view.EnableLoadAccountsRequest(true);
            _view.EnableLoadProxiesRequest(_setting.IsDuoMode || _setting.IsProxiesMode);

            if (e.Error != null)
                _view.ShowMessage(e.Error.Message, MessageType.Error);
        }

        private void LoadAccounts()
        {
            var selectedFile = _view.GetUserFileToOpen(new SelectFileOptions
            {
                Title = "Choose a file containing a list of accounts...",
                Filter = "Text Files (*.txt)|*.txt"
            });

            if (selectedFile == SelectFileResult.Empty)
                return;

            _view.EnableExportRequest(false);

            _view.ClearAccounts();

            _accountManager.Clear();
            _accountsChecker.ResetCounters();

            Parallel.ForEach(File.ReadAllLines(selectedFile.FileName), line =>
            {
                if (!line.Contains(":")) return;

                string email = line.Split(':')[0];
                string pass = line.Split(':')[1];

                if (email.Contains('@') && pass.Length != 0)
                    _accountManager.QueueAccount(email, pass);
            });

            _view.EnableStartRequest(IsCheckAvailable());

            _view.DisplayAccountsStatistic(GetAccountStatistic());
        }

        private bool IsCheckAvailable()
        {
            if (_accountManager.Count <= 0) return false;

            if (_setting.IsDuoMode || _setting.IsProxiesMode)
            {
                return _proxyManager.Count > 0;
            }

            return true;
        }

        private void View_StopCheckingRequested()
        {
            if (_cancellationTokenSource == null) return;

            _cancellationTokenSource.Cancel();
            _view.EnableStopRequest(false);
        }

        private async void View_StartCheckingRequested()
        {
            if (await _accountsChecker.InitCoreAsync())
            {
                _accountsChecker.ResetCounters();

                _view.EnableStopRequest(true);
                _view.EnableLoadAccountsRequest(false);
                _view.EnableLoadProxiesRequest(false);
                _view.EnableStartRequest(false);
                UpdateUIInfos();

                _cancellationTokenSource = new CancellationTokenSource();
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += ProcessStarted;
                worker.RunWorkerCompleted += ProcessCompleted;
                worker.RunWorkerAsync();
            }
            else
            {
                _view.ShowMessage("Please check your DBC Account, Amazed can't log into it.", MessageType.Error);
            }
        }

        private void View_DisplaySettingsRequested()
        {
            var presenter = new SettingsViewPresenter(new frmSettings(), _captchaService, _settingsService);
            presenter.Start();
        }

        private void View_AboutRequested()
        {
            var presenter = new AboutViewPresenter(new frmAbout());
            presenter.Start();
        }

        public void Start()
        {
            _view.EnableLoadProxiesRequest(_setting.IsDuoMode || _setting.IsProxiesMode);

            _view.Show();
        }

        public void Handle(BalanceRetrievedMessage message)
        {
            _view.ShowBalance(message.Balance);
        }

        public void Handle(SettingChangedMessage message)
        {
            _view.EnableLoadProxiesRequest(_setting.IsDuoMode || _setting.IsProxiesMode);
        }

        public void Handle(ProxiesStatisticMessage message)
        {
            _view.DisplayProxiesStatistic(message.Statistic);
        }
    }
}