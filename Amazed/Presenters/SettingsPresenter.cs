using System;
using System.Windows.Forms;
using DreamAmazon.Interfaces;
using DreamAmazon.Models;

namespace DreamAmazon.Presenters
{
    public class SettingsViewPresenter : BasePresenter
    {
        private readonly ISettingsView _view;
        private readonly ICaptchaService _captchaService;
        private readonly ISettingsService _settingsService;
        public SettingModel Settings;

        public SettingsViewPresenter(ISettingsView view, ICaptchaService captchaService, ISettingsService settingsService)
        {
            Contracts.Require(view != null);
            Contracts.Require(captchaService != null);

            _view = view;
            _captchaService = captchaService;
            _settingsService = settingsService;

            Settings = LoadSettings();
            Settings.PropertyChanged += SettingsPropertyChanged;

            _view.ValidateAccount += View_ValidateAccount;
        }

        public async void View_ValidateAccount()
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
            var s = _settingsService.GetSettings();
            
            setting.ShortOutput = s.ShortOutput;
            setting.CleanOutput = s.CleanOutput;
            setting.DBCUser = s.DBCUser;
            setting.DBCPass = s.DBCPass;
            setting.ThreadsCount = s.ThreadsCount;
            setting.UseSecureProxies = s.UseSecureProxies;
            setting.SettingMode = s.SettingMode;
        }

        public void SaveSettings()
        {
            _settingsService.SetSettings(Settings);
        }

        public void Start()
        {
            if (IsViewActive("frmSettings"))
            {
                LoadSettings(Settings);
                _view.BindSettings(Settings);
                _view.Show();
            }
        }
    }
}