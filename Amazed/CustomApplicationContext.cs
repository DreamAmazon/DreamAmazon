using System;
using System.ComponentModel;
using System.Windows.Forms;
using DreamAmazon.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace DreamAmazon
{
    public class CustomApplicationContext : ApplicationContext
    {
        private bool _isLicenseValid;
        private bool _isLicenseValidated;

        public static CustomApplicationContext Current { get; private set; }

        public static CustomApplicationContext Create(Form splash)
        {
            if (Current == null)
            {
                Current = new CustomApplicationContext(splash);
            }
            return Current;
        }

        private CustomApplicationContext(Form splash)
            : base(splash)
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.LicenseKey))
            {
                CloseForm(splash);
                // ask user about license key
                var frmLicense = CreateForm<LicenseForm>();
                frmLicense.Closed += (s, e) =>
                {
                    if (!frmLicense.RealClose)
                    {
                        // closed with correct license key
                        var frmMain = CreateForm<Main>();
                        frmMain.Closed += (_, __) =>
                        {
                            Properties.Settings.Default.Save();
                            Application.Exit();
                        };
                    }
                    else
                    {
                        Application.Exit();
                    }
                };
            }
            else
            {
                // display splash screen
                //var splash = CreateForm<SplashForm>();

                // start init() in background
                var worker = new BackgroundWorker();
                worker.DoWork += (s, e) =>
                {
                    e.Result = false;
                    var initResult = License.Init();
                    e.Result = initResult;
                };
                worker.RunWorkerCompleted += (s, e) =>
                {
                    _isLicenseValidated = true;
                    if (e.Error == null)
                    {
                        var initResult = (bool) e.Result;
                        if (initResult)
                        {
                            // init result is true so we close splash screen and show main form
                            _isLicenseValid = true;
                            CloseForm(splash);
                            var frmMain = CreateForm<Main>();
                            frmMain.Closed += (_, __) => 
                            {
                                Properties.Settings.Default.Save();
                                Application.Exit();
                            };
                            return;
                        }
                    }
                    else
                    {
                        ServiceLocator.Current.GetInstance<ILogger>().Error(e.Error);
                    }

                    // init result is false or error occurred, we should display message and close application
                    MessageBox.Show("Error while initializing components !", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                    Application.Exit();
                };
                worker.RunWorkerAsync();
            }
        }

        private void CloseForm(Form form)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(new Action(() => CloseForm(form)));
            }
            else
            {
                form.Opacity = 0;
                form.ShowInTaskbar = false;
            }
        }

        private T CreateForm<T>() where T : Form
        {
            Form form = (Form)Activator.CreateInstance(typeof(T));
            //MainForm = form;
            form.Show();
            return (T)form;
        }

        protected override void OnMainFormClosed(object sender, EventArgs e)
        {
            if (sender is SplashForm)
            {
                if (!_isLicenseValidated)
                {
                    return;
                }
                if (_isLicenseValid)
                {
                    CreateForm<Main>();
                    return;
                }
            }
            base.OnMainFormClosed(sender, e);
        }

        public void ThrowOnUI(Exception exception)
        {
            //todo: don't know how to do it correctly

            Application.OnThreadException(exception);

            if (MainForm.InvokeRequired)
            {
                ThrowOnUI(exception);
            }
            else
            {
                throw exception;
            }
        }
    }
}