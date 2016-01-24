using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DreamAmazon.Interfaces;

namespace DreamAmazon
{
    public partial class frmMain : BaseView, IMainView
    {
        public event VoidHandler StartCheckingRequested;
        public event VoidHandler StopCheckingRequested;
        public event VoidHandler DisplaySettingsRequested;
        public event VoidHandler AboutRequested;
        public event VoidHandler ShowProxiesRequested;
        public event VoidHandler LoadAccountsRequested;
        public event VoidHandler<bool> CopyToClipboardRequested;
        public event VoidHandler<bool> ExportToFileRequested;

        public frmMain()
        {
            InitializeComponent();
        }

        private void checkBtn_Click(object sender, EventArgs e)
        {
            OnStart();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            OnStop();
        }

        public void DisplayAccount(Account account)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => DisplayAccount(account)));
            }
            else
            {
                ListViewItem li = new ListViewItem(account.Email);
                li.SubItems.Add(account.Password);
                li.SubItems.Add(account.GiftCardBalance);
                li.SubItems.Add(account.Orders);
                li.SubItems.Add(account.ZipCode);
                li.SubItems.Add(account.Phone);
                listView1.Items.Add(li);
            }
        }

        public void EnableStartRequest(bool b)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => EnableStartRequest(b)));
            }
            else
            {
                checkBtn.Enabled = b;
            }
        }

        public IEnumerable<Account> GetAccounts()
        {
            return from ListViewItem li in listView1.Items select GetAccount(li);
        }

        public IEnumerable<Account> GetSelectedAccounts()
        {
            return from ListViewItem li in listView1.SelectedItems select GetAccount(li);
        }

        public void ClearAccounts()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ClearAccounts()));
            }
            else
            {
                listView1.Items.Clear();
            }
        }

        public void ShowBalance(double balance)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowBalance(balance)));
            }
            else
            {
                toolStripStatusLabel2.Text = $"DeathByCaptcha Balance : ${balance}";
            }
        }

        private Account GetAccount(ListViewItem li)
        {
            string email = li.Text;
            string password = li.SubItems[1].Text == "" ? "None" : li.SubItems[1].Text;
            string gc = li.SubItems[2].Text == "" ? "None" : li.SubItems[2].Text;
            string orders = li.SubItems[3].Text == "" ? "None" : li.SubItems[3].Text;
            string zip = li.SubItems[4].Text == "" ? "None" : li.SubItems[4].Text;
            string phone = li.SubItems[5].Text == "" ? "None" : li.SubItems[5].Text;

            return new Account(email, password)
            {
                GiftCardBalance = gc,
                Orders = orders,
                ZipCode = zip,
                Phone = phone
            };
        }

        public void EnableLoadAccountsRequest(bool b)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => EnableLoadAccountsRequest(b)));
            }
            else
            {
                loadAccBtn.Enabled = b;
            }
        }

        public void EnableLoadProxiesRequest(bool b)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => EnableLoadProxiesRequest(b)));
            }
            else
            {
                loadProxiesBtn.Visible = b;
            }
        }

        public void DisplayAccountsStatistic(AccountsStatistic stat)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => DisplayAccountsStatistic(stat)));
            }
            else
            {
                label1.Text = $"Loaded Accounts : {stat.Total}";
                label2.Text = $"Checked Accounts : {stat.Checked}";
                label3.Text = $"Good Accounts : {stat.Valid}";
                label4.Text = $"Bad Accounts : {stat.Bad}";

                if (stat.Threads > 0)
                {
                    checkStatusLbl.Text = $"Checking {stat.Threads} account{(stat.Threads > 1 ? "s" : "")}...";
                }
                else
                {
                    checkStatusLbl.Text = string.Empty;
                }
                toolStripProgressBar1.Maximum = stat.Total;

                if (stat.Checked >= toolStripProgressBar1.Minimum &&
                    stat.Checked <= toolStripProgressBar1.Maximum)
                {
                    toolStripProgressBar1.Value = stat.Checked;
                }

                percentLbl.Text = $"- {stat.Checked*100/stat.Total}%";

                loadAccBtn.Text = $"Load Accounts ({stat.Total})";
            }
        }

        public void DisplayProxiesStatistic(ProxiesStatistic stat)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => DisplayProxiesStatistic(stat)));
            }
            else
            {
                loadProxiesBtn.Text = $"Proxies ({stat.Total})";
            }
        }

        public void EnableExportRequest(bool b)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => EnableExportRequest(b)));
            }
            else
            {
                exportBtn.Visible = b;
            }
        }

        public void EnableStopRequest(bool b)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => EnableStopRequest(b)));
            }
            else
            {
                toolStripProgressBar1.Visible = b;
                percentLbl.Visible = b;
                btnStop.Visible = b;
            }
        }

        private void btnProxies_Click(object sender, EventArgs e)
        {
            OnShowProxiesRequested();
        }

        private void loadAccBtn_Click(object sender, EventArgs e)
        {
            OnLoadAccountsRequested();
        }

        private void ExportToFile(bool detailed)
        {
            OnExportToFileRequested(detailed);
        }

        private void exportBtn_Click(object sender, EventArgs e)
        {
            contextMenuStrip2.Show(MousePosition);
        }

        private void details_Click(object sender, EventArgs e)
        {
            OnCopyToClipboardRequested(true);
        }

        private void singleLine_Click(object sender, EventArgs e)
        {
            OnCopyToClipboardRequested(false);
        }

        private void minimalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportToFile(false);
        }

        private void detailedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportToFile(true);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnAboutRequested();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnDisplaySettingsRequested();
        }

        public void ShowStatusInfo(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowStatusInfo(text)));
            }
            else
            {
                toolStripStatusLabel2.Text = text;
            }
        }

        protected virtual void OnStart()
        {
            StartCheckingRequested?.Invoke();
        }

        protected virtual void OnStop()
        {
            StopCheckingRequested?.Invoke();
        }

        protected virtual void OnDisplaySettingsRequested()
        {
            DisplaySettingsRequested?.Invoke();
        }

        protected virtual void OnAboutRequested()
        {
            AboutRequested?.Invoke();
        }

        protected virtual void OnShowProxiesRequested()
        {
            ShowProxiesRequested?.Invoke();
        }

        protected virtual void OnLoadAccountsRequested()
        {
            LoadAccountsRequested?.Invoke();
        }

        protected virtual void OnCopyToClipboardRequested(bool detailed)
        {
            CopyToClipboardRequested?.Invoke(detailed);
        }

        protected virtual void OnExportToFileRequested(bool detailed)
        {
            ExportToFileRequested?.Invoke(detailed);
        }
    }
}
