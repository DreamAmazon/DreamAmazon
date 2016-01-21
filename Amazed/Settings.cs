using System;
using System.Windows.Forms;
using DreamAmazon.Interfaces;

namespace DreamAmazon
{
    public partial class Settings : Form
    {
        public delegate void SettingsSavedDelegate(bool showProxies);
        public event SettingsSavedDelegate OnSettingsSaved;

        public ICaptchaService CaptchaService { get; set; }

        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            textBox1.Text = Properties.Settings.Default.ShortOutput;
            textBox2.Text = Properties.Settings.Default.CleanOutput;
            textBox3.Text = Properties.Settings.Default.DBCUser;
            textBox4.Text = Properties.Settings.Default.DBCPass;

            numericUpDown1.Value = Properties.Settings.Default.Threads;

            switch (Properties.Settings.Default.Mode)
            {
                case (int)SettingMode.DuoMode:
                    duoMode.Checked = true;
                    break;
                case (int)SettingMode.DbcMode:
                    dbcMode.Checked = true;
                    break;
                case (int)SettingMode.ProxiesMode:
                    proxiesMode.Checked = true;
                    break;
            }

            if (Properties.Settings.Default.ProxiesLogin)
                rbAuthenticatedProxies.Checked = true;
            else
                rbStandardProxies.Checked = true;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.deathbycaptcha.com/");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DBCUser = "";
            Properties.Settings.Default.DBCPass = "";

            button1.Enabled = false;

            var loginResult = await CaptchaService.LoginAsync(textBox3.Text, textBox4.Text);

            if (!loginResult.IsFail)
            {
                Properties.Settings.Default.DBCUser = textBox3.Text;
                Properties.Settings.Default.DBCPass = textBox4.Text;

                MessageBox.Show("Your DBC Account has been validated !", 
                    "DreamAmazon", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("This DBC Account is not valid, please confirm your login details !", 
                    "DreamAmazon", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            button1.Enabled = true;
            Properties.Settings.Default.Save();
        }

        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.ShortOutput = textBox1.Text;
            Properties.Settings.Default.CleanOutput = textBox2.Text;
            Properties.Settings.Default.Threads = Convert.ToInt32(numericUpDown1.Value);

            bool showProxiesBtn = true;

            if (duoMode.Checked)
                Properties.Settings.Default.Mode = (int)SettingMode.DuoMode;
            else if (dbcMode.Checked)
            {
                Properties.Settings.Default.Mode = (int)SettingMode.DbcMode;
                showProxiesBtn = false;
            }
            else if (proxiesMode.Checked)
                Properties.Settings.Default.Mode = (int)SettingMode.ProxiesMode;

            if (rbStandardProxies.Checked)
                Properties.Settings.Default.ProxiesLogin = false;
            else
                Properties.Settings.Default.ProxiesLogin = true;

            Properties.Settings.Default.Save();
            OnSettingsSaved(showProxiesBtn);
        }

        private void label13_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Standard Proxies -> IP:Port\nAuthenticated Proxies -> IP:Port:Username:Password", "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void duoMode_CheckedChanged(object sender, EventArgs e)
        {
            if (duoMode.Checked || proxiesMode.Checked)
                panel1.Visible = true;
            else
                panel1.Visible = false;
        }
    }
}