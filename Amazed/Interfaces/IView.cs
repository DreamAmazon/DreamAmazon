using System;
using DreamAmazon.Models;

namespace DreamAmazon.Interfaces
{
    public delegate void VoidHandler();

    public interface IBaseView
    {
        void Show();
        void ShowMessage(string text, MessageType type);
    }

    public interface IMainView : IBaseView
    {
        void ShowStatusInfo(string text);
    }

    public interface ISettingsView : IBaseView
    {
        void BindSettings(SettingModel setting);
        event VoidHandler ValidateAccount;
    }

    public interface ILicenseView : IBaseView
    {
        void BindSettings(LicenseModel license);
        event EventHandler<string> ValidateLicense;
        void DisableFileds();
        void EnableFields();
        void SetValidationEnable(bool b);
    }
}