using System.ComponentModel;
using System.Runtime.CompilerServices;
using DreamAmazon.Annotations;

namespace DreamAmazon.Models
{
    public class SettingModel : INotifyPropertyChanged
    {
        private string _shortOutput;
        private string _cleanOutput;
        private string _dbcUser;
        private string _dbcPass;
        private decimal _threadsCount;
        private bool _useSecureProxies;
        private SettingMode _settingMode;

        public string ShortOutput
        {
            get { return _shortOutput; }
            set
            {
                if (value == _shortOutput) return;
                _shortOutput = value;
                OnPropertyChanged();
            }
        }

        public string CleanOutput
        {
            get { return _cleanOutput; }
            set
            {
                if (value == _cleanOutput) return;
                _cleanOutput = value;
                OnPropertyChanged();
            }
        }

        public string DBCUser
        {
            get { return _dbcUser; }
            set
            {
                if (value == _dbcUser) return;
                _dbcUser = value;
                OnPropertyChanged();
            }
        }

        public string DBCPass
        {
            get { return _dbcPass; }
            set
            {
                if (value == _dbcPass) return;
                _dbcPass = value;
                OnPropertyChanged();
            }
        }

        public decimal ThreadsCount
        {
            get { return _threadsCount; }
            set
            {
                if (value == _threadsCount) return;
                _threadsCount = value;
                OnPropertyChanged();
            }
        }

        public bool UseSecureProxies
        {
            get { return _useSecureProxies; }
            set
            {
                if (value == _useSecureProxies) return;
                _useSecureProxies = value;
                OnPropertyChanged();
            }
        }

        public bool UseStandardProxies
        {
            get { return !UseSecureProxies; }
            set
            {
                if (value == !UseSecureProxies) return;
                UseSecureProxies = !value;
                OnPropertyChanged();
            }
        }

        public bool IsDuoMode
        {
            get { return SettingMode == SettingMode.DuoMode; }
            set { SettingMode = value ? SettingMode.DuoMode : SettingMode.None; }
        }

        public bool IsDbcMode
        {
            get { return SettingMode == SettingMode.DbcMode; }
            set { SettingMode = value ? SettingMode.DbcMode : SettingMode.None; }
        }

        public bool IsProxiesMode
        {
            get { return SettingMode == SettingMode.ProxiesMode; }
            set { SettingMode = value ? SettingMode.ProxiesMode : SettingMode.None; }
        }

        public SettingMode SettingMode
        {
            get { return _settingMode; }
            set
            {
                if (value == _settingMode) return;
                _settingMode = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}