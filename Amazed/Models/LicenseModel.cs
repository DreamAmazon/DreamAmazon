using System.ComponentModel;
using System.Runtime.CompilerServices;
using DreamAmazon.Annotations;

namespace DreamAmazon.Models
{
    public class LicenseModel : INotifyPropertyChanged
    {
        private string _licenseKey;

        public string LicenseKey
        {
            get { return _licenseKey; }
            set
            {
                if (value == _licenseKey) return;
                _licenseKey = value;
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