using DreamAmazon.Models;

namespace DreamAmazon.Events
{
    public class SettingChangedMessage
    {
        public SettingModel Setting { get; protected set; }

        public SettingChangedMessage(SettingModel setting)
        {
            Setting = setting;
        }
    }
}