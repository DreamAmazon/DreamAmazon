using System.Windows.Forms;

namespace DreamAmazon
{
    public abstract class BaseView : Form
    {
        public void ShowMessage(string text, MessageType type)
        {
            MessageBoxIcon icon = MessageBoxIcon.None;

            if (type == MessageType.Info)
                icon = MessageBoxIcon.Information;
            else if (type == MessageType.Error)
                icon = MessageBoxIcon.Error;
            else if (type == MessageType.Warning)
                icon = MessageBoxIcon.Warning;

            MessageBox.Show(text, "DreamAmazon", MessageBoxButtons.OK, icon);
        }
    }
}