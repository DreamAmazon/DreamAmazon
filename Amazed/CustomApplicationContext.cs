using System;
using System.ComponentModel;
using System.Windows.Forms;
using DreamAmazon.Interfaces;
using DreamAmazon.Presenters;
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
            return Current ?? (Current = new CustomApplicationContext(splash));
        }

        private void ShowMainView()
        {
            var form = new frmMain();
            var presenter = new MainViewPresenter(form);
            MainForm = form;
            presenter.Start();
        }

        private CustomApplicationContext(Form splash)
            : base(splash)
        {
#if DEBUG
            Globals.ProcessBypass();
            ShowMainView();
            return;
#endif

            if (string.IsNullOrEmpty(Properties.Settings.Default.LicenseKey))
            {
                // ask user about license key
                CreateMainForm<LicenseForm>();
            }
            else
            {
                // display splash screen
                //var splash = CreateMainForm<SplashForm>();

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
                            return;
                        }
                    }
                    else
                    {
                        ServiceLocator.Current.GetInstance<ILogger>().Error(e.Error);
                    }

                    // init result is false or error occurred, we should display message and close application
                    MessageBox.Show(splash, "Error while initializing components !", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                    CloseForm(splash);
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
                form.Close();
            }
        }

        private T CreateMainForm<T>() where T : Form
        {
            Form form = (Form)Activator.CreateInstance(typeof(T));
            MainForm = form;
            MainForm.Show();
            return (T)MainForm;
        }

        protected override void OnMainFormClosed(object sender, EventArgs e)
        {
            if (sender is LicenseForm)
            {
                if (!(sender as LicenseForm).RealClose)
                {
                    // closed with correct license key
                    ShowMainView();
                    return;
                }
            }
            else if (sender is SplashForm)
            {
                if (_isLicenseValidated && _isLicenseValid)
                {
                    ShowMainView();
                    return;
                }
            }
            else if (sender is frmMain)
            {
                Properties.Settings.Default.Save();
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