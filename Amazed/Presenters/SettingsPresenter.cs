using DreamAmazon.Events;
using DreamAmazon.Interfaces;
using DreamAmazon.Models;
using EventAggregatorNet;
using Microsoft.Practices.ServiceLocation;

namespace DreamAmazon.Presenters
{
    public class SettingsViewPresenter : BasePresenter
    {
        private readonly ISettingsView _view;
        private readonly ICaptchaService _captchaService;
        private readonly ISettingsService _settingsService;
        public SettingModel Settings;
        private readonly IEventAggregator _eventAggregator;

        public SettingsViewPresenter(ISettingsView view, ICaptchaService captchaService, ISettingsService settingsService)
        {
            Contracts.Require(view != null);
            Contracts.Require(captchaService != null);

            _view = view;
            _captchaService = captchaService;
            _settingsService = settingsService;
            _eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();

            Settings = LoadSettings();
            Settings.PropertyChanged += SettingsPropertyChanged;

            _view.ValidateAccountRequested += View_ValidateAccountRequested;
        }

        public async void View_ValidateAccountRequested()
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

            UpdateSettings();
        }

        private void SettingsPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateSettings();

            if (e.PropertyName == "DBCUser" || e.PropertyName == "DBCPass")
            {
                ValidateAccountInfo(Settings);
            }

            _eventAggregator.SendMessage(new SettingChangedMessage(Settings));
        }

        private void ValidateAccountInfo(SettingModel setting)
        {
            var notification = Validator.ValidateObject(setting);

            var m1 = notification.GetMessages("DBCUser");
            var m2 = notification.GetMessages("DBCPass");

            _view.EnableValidateAccount(m1.Length == 0 && m2.Length == 0);
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

            ValidateAccountInfo(setting);
        }

        public void UpdateSettings()
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