using System;
using DreamAmazon.Models;

namespace DreamAmazon.Interfaces
{
    public interface ISettingsView
    {
        void Show();
        void BindSettings(SettingModel setting);
        event EventHandler ValidateAccount;
    }
}