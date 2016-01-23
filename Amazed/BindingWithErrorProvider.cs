using System.ComponentModel;
using System.Windows.Forms;

namespace DreamAmazon
{
    public class BindingWithErrorProvider : Binding
    {
        private readonly ErrorProvider _errorProvider;
        private readonly string _dataMember;

        public BindingWithErrorProvider(string propertyName, object dataSource, string dataMember,
            bool formattingEnabled, DataSourceUpdateMode dataSourceUpdateMode, ErrorProvider errorProvider)
            : base(propertyName, dataSource, dataMember, formattingEnabled, dataSourceUpdateMode)
        {
            _errorProvider = errorProvider;
            _dataMember = dataMember;

            var notify = dataSource as INotifyPropertyChanged;
            if (notify != null)
            {
                notify.PropertyChanged += Notify_PropertyChanged;
            }
        }

        private void Notify_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != _dataMember)
                return;

            if (Control == null)
                return;

            var notification = Validator.ValidateObject(sender);
            var messages = notification.GetMessages(e.PropertyName);

            if (messages.Length == 0)
            {
                _errorProvider.Clear();
            }
            else
            {
                foreach (var message in messages)
                {
                    _errorProvider.SetError(Control, message);
                }
            }
        }
    }
}