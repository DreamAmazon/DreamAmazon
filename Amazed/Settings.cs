using System;
using System.Windows.Forms;
using DreamAmazon.Interfaces;
using DreamAmazon.Models;

namespace DreamAmazon
{
    public partial class Settings : Form, ISettingsView
    {
        public event EventHandler ValidateAccount;

        public void BindSettings(SettingModel setting)
        {
            //ShortOutput = setting.ShortOutput;
            textBox1.DataBindings.Add("Text", setting, "ShortOutput");
            //CleanOutput = setting.CleanOutput;
            textBox2.DataBindings.Add("Text", setting, "CleanOutput");
            //DBCUser = setting.DBCUser;
            textBox3.DataBindings.Add("Text", setting, "DBCUser");
            //DBCPass = setting.DBCPass;
            textBox4.DataBindings.Add("Text", setting, "DBCPass");
            //ThreadsCount = setting.ThreadsCount;
            numericUpDown1.DataBindings.Add("Value", setting, "ThreadsCount");
            UseSecureProxies = setting.UseSecureProxies;
            SettingMode = setting.SettingMode;
        }

        public bool UseSecureProxies
        {
            get { return rbAuthenticatedProxies.Checked; }
            set
            {
                if (value)
                    rbAuthenticatedProxies.Checked = true;
                else
                    rbStandardProxies.Checked = true;
            }
        }

        public SettingMode SettingMode
        {
            get
            {
                if (duoMode.Checked)
                    return SettingMode.DuoMode;
                if (dbcMode.Checked)
                    return SettingMode.DbcMode;
                if (proxiesMode.Checked)
                    return SettingMode.ProxiesMode;
                throw new ApplicationException("unknown setting mode selected");
            }
            set
            {
                switch (value)
                {
                    case SettingMode.DuoMode:
                        duoMode.Checked = true;
                        break;
                    case SettingMode.DbcMode:
                        dbcMode.Checked = true;
                        break;
                    case SettingMode.ProxiesMode:
                        proxiesMode.Checked = true;
                        break;
                    default:
                        throw new ArgumentException("unknown setting mode");
                }
            }
        }

        public Settings()
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
            ValidateAccount?.Invoke(this, EventArgs.Empty);
        }
    }
}