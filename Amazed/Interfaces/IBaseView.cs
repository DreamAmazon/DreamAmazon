namespace DreamAmazon.Interfaces
{
    public interface IBaseView
    {
        void Show();
        void ShowMessage(string text, MessageType type);
    }
}