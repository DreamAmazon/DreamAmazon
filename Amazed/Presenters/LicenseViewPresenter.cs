using System;
using DreamAmazon.Interfaces;
using DreamAmazon.Models;
using Microsoft.Practices.ServiceLocation;

namespace DreamAmazon.Presenters
{
    public class LicenseViewPresenter : BasePresenter
    {
        private readonly ILicenseView _view;

        public bool RealClose { get; protected set; }

        public SettingModel Setting;
        private readonly ILogger _logger;

        public LicenseViewPresenter(ILicenseView view)
        {
            Contracts.Require(view != null);

            var settingsService = ServiceLocator.Current.GetInstance<ISettingsService>();
            Setting = settingsService.GetSettings();
            _logger = ServiceLocator.Current.GetInstance<ILogger>();

            _view = view;
            Setting.PropertyChanged += License_PropertyChanged;

            _view.ValidateLicenseRequested += View_ValidateLicenseRequested;

            RealClose = true;
        }

        private async void View_ValidateLicenseRequested(object sender, string e)
        {
            _view.DisableFileds();

            bool initResult;

            try
            {
                initResult = await License.InitAsync(Setting.LicenseKey);
            }
            catch (Exception exception)
            {
                _logger.Error(exception);

                _view.ShowMessage("Error while initializing components !", MessageType.Error);

                RealClose = true;
                _view.EnableFields();
                return;
            }

            if (!initResult)
            {
                _view.ShowMessage("Your license key is invalid !", MessageType.Error);
                Setting.LicenseKey = string.Empty;
                RealClose = true;
            }
            else
            {
                RealClose = false;
            }

            _view.EnableFields();
        }

        private void License_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LicenseKey")
            {
                var notifications = Validator.ValidateObject(Setting);
                _view.EnableValidateLicense(notifications.GetMessages(e.PropertyName).Length == 0);
            }
        }

        public void Start()
        {
            if (IsViewActive("frmLicense"))
            {
                _view.BindSettings(Setting);
                _view.Show();
            }
        }
    }
}