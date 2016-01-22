using System;
using DreamAmazon.Models;

namespace DreamAmazon.Interfaces
{
    public interface ISettingsView : IBaseView
    {
        void BindSettings(SettingModel setting);
        event EventHandler ValidateAccount;
    }
}