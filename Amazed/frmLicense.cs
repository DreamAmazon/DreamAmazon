using System;
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

        public void SetValidationEnable(bool b)
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

        public void BindSettings(LicenseModel license)
        {
            textBox1.DataBindings.Add("Text", license, "LicenseKey");
        }

        protected virtual void OnValidateLicense(string e)
        {
            ValidateLicense?.Invoke(this, e);
        }
    }
}
