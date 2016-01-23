using System;
using DreamAmazon.Interfaces;
using DreamAmazon.Models;

namespace DreamAmazon.Services
{
    public class DefaultSettingsService : ISettingsService
    {
        private readonly SettingModel _setting;

        public DefaultSettingsService()
        {
            _setting = LoadSettings();
        }

        private SettingModel LoadSettings()
        {
            var setting = new SettingModel();
            setting.ShortOutput = Properties.Settings.Default.ShortOutput;
            setting.CleanOutput = Properties.Settings.Default.CleanOutput;
            setting.DBCUser = Properties.Settings.Default.DBCUser;
            setting.DBCPass = Properties.Settings.Default.DBCPass;
            setting.ThreadsCount = Properties.Settings.Default.Threads;
            setting.UseSecureProxies = Properties.Settings.Default.ProxiesLogin;
            setting.SettingMode = (SettingMode)Properties.Settings.Default.Mode;
            return setting;
        }

        public SettingModel GetSettings()
        {
            return _setting;
        }

        public void SetSettings(SettingModel setting)
        {
            MapToInnerSettigns(setting);
        }

        public void Save()
        {
            ApplySettings();
            Properties.Settings.Default.Save();
        }

        private void MapToInnerSettigns(SettingModel setting)
        {
            _setting.ShortOutput = setting.ShortOutput;
            _setting.CleanOutput = setting.CleanOutput;
            _setting.DBCUser = setting.DBCUser;
            _setting.DBCPass = setting.DBCPass;
            _setting.ThreadsCount = setting.ThreadsCount;
            _setting.UseSecureProxies = setting.UseSecureProxies;
            _setting.SettingMode = setting.SettingMode;
        }

        private void ApplySettings()
        {
            Properties.Settings.Default.ShortOutput = _setting.ShortOutput;
            Properties.Settings.Default.CleanOutput = _setting.CleanOutput;
            Properties.Settings.Default.DBCUser = _setting.DBCUser;
            Properties.Settings.Default.DBCPass = _setting.DBCPass;
            Properties.Settings.Default.Threads = Convert.ToInt32(_setting.ThreadsCount);
            Properties.Settings.Default.ProxiesLogin = _setting.UseSecureProxies;
            Properties.Settings.Default.Mode = (int)_setting.SettingMode;
        }
    }
}