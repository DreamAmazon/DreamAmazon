namespace DreamAmazon.Interfaces
{
    public interface ILogger
    {
        void Error(System.Exception exception);
        void Debug(string text);
        void Info(string text);
    }
}