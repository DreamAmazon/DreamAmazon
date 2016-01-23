using System;
using System.Windows.Forms;
using DreamAmazon.Interfaces;
using DreamAmazon.Models;

namespace DreamAmazon
{
    public partial class frmLicense : BaseView, ILicenseView
    {
        public event EventHandler<string> ValidateLicense;

        public void DisableFileds()
        {
            button1.Enabled = false;
            textBox1.Enabled = false;
        }

        public void EnableFields()
        {
            button1.Enabled = true;
            textBox1.Enabled = true;
        }

        public void EnableValidateLicense(bool b)
        {
            button1.Enabled = b;
        }

        public frmLicense()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OnValidateLicense(textBox1.Text);
        }

        public void BindSettings(SettingModel setting)
        {
            textBox1.DataBindings.Add(new BindingWithErrorProvider("Text", setting, "LicenseKey", true, DataSourceUpdateMode.OnPropertyChanged, errorProvider1));
        }

        protected virtual void OnValidateLicense(string e)
        {
            ValidateLicense?.Invoke(this, e);
        }
    }
}
