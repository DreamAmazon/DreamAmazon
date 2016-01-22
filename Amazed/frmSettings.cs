using System;
using System.Windows.Forms;
using DreamAmazon.Interfaces;
using DreamAmazon.Models;

namespace DreamAmazon
{
    public partial class frmSettings : BaseView, ISettingsView
    {
        public event VoidHandler ValidateAccount;

        public void BindSettings(SettingModel setting)
        {
            textBox1.DataBindings.Add("Text", setting, "ShortOutput");
            textBox2.DataBindings.Add("Text", setting, "CleanOutput");
            textBox3.DataBindings.Add("Text", setting, "DBCUser");
            textBox4.DataBindings.Add("Text", setting, "DBCPass");
            numericUpDown1.DataBindings.Add("Value", setting, "ThreadsCount");
            rbAuthenticatedProxies.DataBindings.Add("Checked", setting, "UseSecureProxies");
            rbStandardProxies.DataBindings.Add("Checked", setting, "UseStandardProxies");
            duoMode.DataBindings.Add("Checked", setting, "IsDuoMode");
            dbcMode.DataBindings.Add("Checked", setting, "IsDbcMode");
            proxiesMode.DataBindings.Add("Checked", setting, "IsProxiesMode");
        }

        public frmSettings()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.deathbycaptcha.com/");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OnValidateAccount();
        }

        private void duoMode_CheckedChanged(object sender, EventArgs e)
        {
            panel1.Visible = duoMode.Checked || proxiesMode.Checked;
        }

        private void label13_Click(object sender, EventArgs e)
        {
            var msg = "Standard Proxies -> IP:Port\nAuthenticated Proxies -> IP:Port:Username:Password";
            MessageBox.Show(msg, "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected virtual void OnValidateAccount()
        {
            ValidateAccount?.Invoke();
        }
    }
}