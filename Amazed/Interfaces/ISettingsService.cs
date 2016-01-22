using DreamAmazon.Models;

namespace DreamAmazon.Interfaces
{
    public interface ISettingsService
    {
        SettingModel GetSettings();
        void SetSettings(SettingModel setting);
        void Save();
    }
}