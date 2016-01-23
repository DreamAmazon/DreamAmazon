namespace DreamAmazon
{
    public class NotificationMessage
    {
        private readonly string _fieldName;
        private readonly string _message;

        public NotificationMessage(string fieldName, string message)
        {
            _fieldName = fieldName;
            _message = message;
        }

        public string FieldName
        {
            get { return _fieldName; }
        }

        public string Message
        {
            get { return _message; }
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            NotificationMessage notificationMessage = obj as NotificationMessage;
            if (notificationMessage == null) return false;
            return Equals(_fieldName, notificationMessage._fieldName) && Equals(_message, notificationMessage._message);
        }

        protected bool Equals(NotificationMessage other)
        {
            return string.Equals(_fieldName, other._fieldName) && string.Equals(_message, other._message);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_fieldName != null ? _fieldName.GetHashCode() : 0)*397) ^ (_message != null ? _message.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.Format("Field {0}: {1}", _fieldName, _message);
        }
    }
}