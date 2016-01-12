using System;
using System.Windows.Forms;
using DreamAmazon.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace DreamAmazon
{
    public partial class LicenseForm : Form
    {
        public bool RealClose = true;

        public LicenseForm()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = textBox1.Text.Length == 16;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.LicenseKey = textBox1.Text;

            button1.Enabled = false;
            textBox1.Enabled = false;

            bool initResult;

            try
            {
                initResult = await License.InitAsync();
            }
            catch (Exception exception)
            {
                ServiceLocator.Current.GetInstance<ILogger>().Error(exception);
                MessageBox.Show(this, "Error while initializing components !", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                RealClose = true;
                button1.Enabled = true;
                textBox1.Enabled = true;
                Close();
                return;
            }

            if (!initResult)
            {
                MessageBox.Show("Your license key is invalid !", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Properties.Settings.Default.LicenseKey = "";
                RealClose = true;
            }
            else
            {
                RealClose = false;
            }

            button1.Enabled = true;
            textBox1.Enabled = true;
            Close();
        }
    }
}
