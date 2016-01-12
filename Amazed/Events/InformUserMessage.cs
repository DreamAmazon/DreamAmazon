namespace DreamAmazon.Events
{
    public class InformUserMessage
    {
        public enum MessageType
        {
            Error,
            Info
        }
        public class Message
        {
            public string Text { get; protected set; }
            public MessageType Type { get; protected set; }

            public Message(string text, MessageType type)
            {
                Text = text;
                Type = type;
            }
        }

        public Message UserMessage { get; protected set; }

        public InformUserMessage(Message message)
        {
            UserMessage = message;
        }
    }
}