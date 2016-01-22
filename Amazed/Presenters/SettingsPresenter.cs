using System;
using System.Windows.Forms;
using DreamAmazon.Interfaces;
using DreamAmazon.Models;

namespace DreamAmazon.Presenters
{
    public class SettingsViewPresenter
    {
        private readonly ISettingsView _view;
        private readonly ICaptchaService _captchaService;
        public SettingModel Settings;

        public SettingsViewPresenter(ISettingsView view, ICaptchaService captchaService)
        {
            Contracts.Require(view != null);
            Contracts.Require(captchaService != null);

            _view = view;
            _captchaService = captchaService;

            Settings = LoadSettings();
            Settings.PropertyChanged += SettingsPropertyChanged;

            _view.ValidateAccount += View_ValidateAccount;
        }

        private async void View_ValidateAccount(object sender, EventArgs e)
        {
            var loginResult = await _captchaService.LoginAsync(Settings.DBCUser, Settings.DBCPass);

            if (loginResult.Success)
            {
                _view.ShowMessage("Your DBC Account has been validated !", MessageType.Info);
            }
            else
            {
                _view.ShowMessage("This DBC Account is not valid, please confirm your login details !", MessageType.Error);
            }

            SaveSettings();
        }

        private void SettingsPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SaveSettings();
        }

        private SettingModel LoadSettings()
        {
            var settings = new SettingModel();
            LoadSettings(settings);
            return settings;
        }

        private void LoadSettings(SettingModel setting)
        {
            setting.ShortOutput = Properties.Settings.Default.ShortOutput;
            setting.CleanOutput = Properties.Settings.Default.CleanOutput;
            setting.DBCUser = Properties.Settings.Default.DBCUser;
            setting.DBCPass = Properties.Settings.Default.DBCPass;
            setting.ThreadsCount = Properties.Settings.Default.Threads;
            setting.UseSecureProxies = Properties.Settings.Default.ProxiesLogin;
            setting.SettingMode = (SettingMode)Properties.Settings.Default.Mode;
        }

        public void SaveSettings()
        {
            Properties.Settings.Default.ShortOutput = Settings.ShortOutput;
            Properties.Settings.Default.CleanOutput = Settings.CleanOutput;
            Properties.Settings.Default.Threads = Convert.ToInt32(Settings.ThreadsCount);
            Properties.Settings.Default.ProxiesLogin = Settings.UseSecureProxies;
            Properties.Settings.Default.Mode = (int)Settings.SettingMode;
        }

        public void Start()
        {
            if (IsViewActive("Settings"))
            {
                LoadSettings(Settings);
                _view.BindSettings(Settings);
                _view.Show();
            }
        }

        private static bool IsViewActive(string viewName)
        {
            return Application.OpenForms[viewName] == null || Application.OpenForms[viewName].Visible == false;
        }
    }
}