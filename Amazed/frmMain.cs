﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DreamAmazon.Interfaces;
using DreamAmazon.Presenters;
using Microsoft.Practices.ServiceLocation;

namespace DreamAmazon
{
    public partial class frmMain : BaseView, IMainView
    {
        private readonly Checker _accountsChecker;
        private readonly ICaptchaService _captchaService;
        private readonly IProxyManager _proxyManager;
        private readonly IAccountManager _accountManager;
        private readonly ILogger _logger;

        private CancellationTokenSource _cancellationTokenSource;
        private readonly ISettingsService _settingsService;

        public frmMain()
        {
            InitializeComponent();

            _logger = ServiceLocator.Current.GetInstance<ILogger>();
            _captchaService = ServiceLocator.Current.GetInstance<ICaptchaService>();
            _proxyManager = ServiceLocator.Current.GetInstance<IProxyManager>();
            _accountManager = ServiceLocator.Current.GetInstance<IAccountManager>();
            _settingsService = ServiceLocator.Current.GetInstance<ISettingsService>();

            _accountsChecker = new Checker(_captchaService, _proxyManager, _accountManager);

            _accountsChecker.OnCheckCompleted += AccountsCheckerOnCheckCompleted;
        }

        void AccountsCheckerOnCheckCompleted(CheckResults results, Account account)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowResult(results, account)));
            }
            else
            {
                ShowResult(results, account);
            }
        }

        private async void checkBtn_Click(object sender, EventArgs e)
        {
            if (await _accountsChecker.InitCoreAsync())
            {
                toolStripProgressBar1.Maximum = _accountManager.Count;
                toolStripProgressBar1.Value = 0;
                toolStripProgressBar1.Visible = true;
                percentLbl.Text = "- 0%";
                percentLbl.Visible = true;
                loadAccBtn.Enabled = false;
                checkBtn.Enabled = false;
                btnStop.Visible = true;

                UpdateUIInfos();

                _cancellationTokenSource = new CancellationTokenSource();
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += ProcessStarted;
                worker.RunWorkerCompleted += ProcessCompleted;
                worker.RunWorkerAsync();
            }
            else
            {
                ShowMessage("Please check your DBC Account, Amazed can't log into it.", MessageType.Error);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (_cancellationTokenSource == null) return;

            _cancellationTokenSource.Cancel();
            btnStop.Visible = false;
            _cancellationTokenSource = null;
        }

        private void ShowResult(CheckResults results, Account account)
        {
            if (results == CheckResults.Good)
            {
                AddAccountToListView(account);
                exportBtn.Visible = true;

                LogAccountInfos(account);
            }

            UpdateUIInfos();
        }

        private void LogAccountInfos(Account account)
        {
            string output = Properties.Settings.Default.ShortOutput;

            output = output.Replace("{Email}", account.Email)
                .Replace("{Password}", account.Password)
                .Replace("{Balance}", account.GiftCardBalance)
                .Replace("{Order Quantity}", account.Orders)
                .Replace("{Zip}", account.ZipCode)
                .Replace("{Phone}", account.Phone);

            _logger.Info(output);
        }

        private void AddAccountToListView(Account account)
        {
            ListViewItem li = new ListViewItem(account.Email);
            li.SubItems.Add(account.Password);
            li.SubItems.Add(account.GiftCardBalance);
            li.SubItems.Add(account.Orders);
            li.SubItems.Add(account.ZipCode);
            li.SubItems.Add(account.Phone);
            listView1.Items.Add(li);
        }

        private void UpdateUIInfos()
        {
            UpdateAccountCounters();

            checkStatusLbl.Text = string.Format("Checking {0} account{1}...", _accountsChecker.ActiveThreads,
                _accountsChecker.ActiveThreads > 1 ? "s" : "");
            if (_accountsChecker.AccountsChecked >= toolStripProgressBar1.Minimum &&
                _accountsChecker.AccountsChecked <= toolStripProgressBar1.Maximum)
            {
                toolStripProgressBar1.Value = _accountsChecker.AccountsChecked;
            }
            percentLbl.Text = string.Format("- {0}%", _accountsChecker.AccountsChecked*100/_accountManager.Count);
        }

        private void UpdateAccountCounters()
        {
            label1.Text = string.Format("Loaded Accounts : {0}", _accountManager.Count);
            label2.Text = string.Format("Checked Accounts : {0}", _accountsChecker.AccountsChecked);
            label3.Text = string.Format("Good Accounts : {0}", _accountsChecker.ValidAccounts);
            label4.Text = string.Format("Bad Accounts : {0}", _accountsChecker.BadAccounts);
        }

        private void ProcessStarted(object sender, DoWorkEventArgs e)
        {
            _accountsChecker.Start(_cancellationTokenSource.Token);
        }

        private void ProcessCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                checkStatusLbl.Text = "Canceled";
            }
            else
            {
                if (e.Error is OperationCanceledException)
                    checkStatusLbl.Text = "Canceled";
                else
                    checkStatusLbl.Text = "Completed";
            }

            toolStripProgressBar1.Visible = false;
            percentLbl.Visible = false;
            loadAccBtn.Enabled = true;
            checkBtn.Enabled = true;
            loadProxiesBtn.Enabled = true;
            _cancellationTokenSource = null;
            btnStop.Visible = false;

            if (e.Error != null)
                ShowMessage(e.Error.Message, MessageType.Error);
        }

        #region Load Files

        private void loadProxiesBtn_Click(object sender, EventArgs e)
        {
            checkStatusLbl.Text = "Idle";
            LoadProxies();
        }

        private void loadAccBtn_Click(object sender, EventArgs e)
        {
            checkStatusLbl.Text = "Idle";
            LoadAccounts();
        }

        private void LoadAccounts()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Choose a file containing a list of accounts...";
                ofd.Filter = "Text Files (*.txt)|*.txt";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    checkBtn.Enabled = false;
                    exportBtn.Visible = false;
                    listView1.Items.Clear();
                    _accountManager.Clear();
                    _accountsChecker.ResetCounters();
                    
                    foreach (string line in File.ReadAllLines(ofd.FileName))
                    {
                        if (line.Contains(":"))
                        {
                            string email = line.Split(':')[0];
                            string pass = line.Split(':')[1];

                            if (email.Contains('@') && pass.Length != 0)
                                _accountManager.QueueAccount(email, pass);
                        }
                    }
                    loadAccBtn.Text = string.Format("Load Accounts ({0})", _accountManager.Count);

                    if (_accountManager.Count > 0 && (_proxyManager.Count > 0 || !loadProxiesBtn.Visible))
                        checkBtn.Enabled = true;
                    else
                        checkBtn.Enabled = false;

                    UpdateAccountCounters();
                }
            }
        }

        private void LoadProxies()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Choose a file containing a list of proxies...";
                ofd.Filter = "Text Files (*.txt)|*.txt";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _proxyManager.Clear();

                    foreach (var line in File.ReadAllLines(ofd.FileName))
                    {
                        if (line.Contains(":"))
                        {
                            var data = line.Split(':');

                            if (!Properties.Settings.Default.ProxiesLogin)
                                _proxyManager.QueueProxy(data[0], data[1]);
                            else
                                _proxyManager.QueueProxy(data[0], data[1], data[2], data[3]);
                        }
                    }
                    loadProxiesBtn.Text = string.Format("Load Proxies ({0})", _proxyManager.Count);

                    if (_accountManager.Count > 0 && (_proxyManager.Count > 0 || !loadProxiesBtn.Visible))
                        checkBtn.Enabled = true;
                    else
                        checkBtn.Enabled = false;

                    UpdateAccountCounters();
                }
            }
        }

        #endregion

        #region Export Functions

        private void CopyToSelectedFormat(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 0)
            {
                ListViewItem li = listView1.SelectedItems[0];
                string email = li.Text;
                string password = li.SubItems[1].Text;
                string gc = li.SubItems[2].Text;
                string orders = li.SubItems[3].Text == "" ? "None" : li.SubItems[3].Text;
                string zip = li.SubItems[4].Text == "" ? "None" : li.SubItems[4].Text;
                string phone = li.SubItems[5].Text == "" ? "None" : li.SubItems[5].Text;

                string output;

                if ((sender as ToolStripItem).Name == "singleLine")
                {
                    output = Properties.Settings.Default.ShortOutput;
                }
                else
                    output = Properties.Settings.Default.CleanOutput;

                output = output.Replace("{Email}", email);
                output = output.Replace("{Password}", password);
                output = output.Replace("{Balance}", gc);
                output = output.Replace("{Order Quantity}", orders);
                output = output.Replace("{Zip}", zip);
                output = output.Replace("{Phone}", phone);

                Clipboard.SetText(output);
            }
        }

        private void ExportToFile(bool detailed)
        {
            string style;
            string output = "";

            if (detailed)
                style = Properties.Settings.Default.CleanOutput;
            else
                style = Properties.Settings.Default.ShortOutput;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Where do you want to export your accounts ?";
                sfd.FileName = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString().Replace(":", "h ") + " Accounts.txt";
                sfd.Filter = "Text Files (*.txt)|*.txt";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    foreach (ListViewItem li in listView1.Items)
                    {
                        string email = li.Text;
                        string password = li.SubItems[1].Text;
                        string gc = li.SubItems[2].Text;
                        string orders = li.SubItems[3].Text == "" ? "None" : li.SubItems[3].Text;
                        string zip = li.SubItems[4].Text == "" ? "None" : li.SubItems[4].Text;
                        string phone = li.SubItems[5].Text == "" ? "None" : li.SubItems[5].Text;

                        string data = string.Empty;
                        data += style;
                        data = data.Replace("{Email}", email);
                        data = data.Replace("{Password}", password);
                        data = data.Replace("{Balance}", gc);
                        data = data.Replace("{Order Quantity}", orders);
                        data = data.Replace("{Zip}", zip);
                        data = data.Replace("{Phone}", phone);

                        output += data + "\r\n";

                        if (detailed)
                            output += "\r\n";
                    }

                    File.WriteAllText(sfd.FileName, output);
                }
            }
        }

        private void exportBtn_Click(object sender, EventArgs e)
        {
            contextMenuStrip2.Show(MousePosition);
        }

        private void minimalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportToFile(false);
        }

        private void detailedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportToFile(true);
        }

        #endregion

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["About"] == null || Application.OpenForms["About"].Visible == false)
            {
                frmAbout ab = new frmAbout();
                ab.Show();
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var presenter = new SettingsViewPresenter(new frmSettings(), _captchaService, _settingsService);
            presenter.Start();
        }

        public void ShowStatusInfo(string text)
        {
            toolStripStatusLabel2.Text = text;
        }
    }
}