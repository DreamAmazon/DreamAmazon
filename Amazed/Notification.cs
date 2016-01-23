using System.Collections.Generic;

namespace DreamAmazon
{
    public class Notification
    {
        public static readonly string REQUIRED_FIELD = "Required Field";
        public static readonly string INVALID_FORMAT = "Invalid Format";

        private readonly List<NotificationMessage> _list = new List<NotificationMessage>();

        public bool IsValid()
        {
            return _list.Count == 0;
        }

        public void RegisterMessage(string fieldName, string message)
        {
            _list.Add(new NotificationMessage(fieldName, message));
        }

        public string[] GetMessages(string fieldName)
        {
            List<NotificationMessage> messages = _list.FindAll(delegate (NotificationMessage m) { return m.FieldName == fieldName; });
            string[] returnValue = new string[messages.Count];
            for (int i = 0; i < messages.Count; i++)
            {
                returnValue[i] = messages[i].Message;
            }

            return returnValue;
        }

        public NotificationMessage[] AllMessages
        {
            get
            {
                _list.Sort();
                return _list.ToArray();
            }
        }

        public bool HasMessage(string fieldName, string messageText)
        {
            NotificationMessage message = new NotificationMessage(fieldName, messageText);
            return _list.Contains(message);
        }
    }
}