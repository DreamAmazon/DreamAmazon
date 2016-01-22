using System;
using DreamAmazon.Interfaces;
using DreamAmazon.Models;

namespace DreamAmazon.Services
{
    public class DefaultSettingsService : ISettingsService
    {
        public SettingModel GetSettings()
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

        public void SetSettings(SettingModel setting)
        {
            Properties.Settings.Default.ShortOutput = setting.ShortOutput;
            Properties.Settings.Default.CleanOutput = setting.CleanOutput;
            Properties.Settings.Default.DBCUser = setting.DBCUser;
            Properties.Settings.Default.DBCPass = setting.DBCPass;
            Properties.Settings.Default.Threads = Convert.ToInt32(setting.ThreadsCount);
            Properties.Settings.Default.ProxiesLogin = setting.UseSecureProxies;
            Properties.Settings.Default.Mode = (int)setting.SettingMode;
        }

        public void Save()
        {
            Properties.Settings.Default.Save();
        }
    }
}