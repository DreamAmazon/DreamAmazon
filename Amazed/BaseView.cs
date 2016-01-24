using System.Windows.Forms;

namespace DreamAmazon
{
    public class BaseView : Form
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
            else if (type == MessageType.Status)
            {

                return;
            }

            MessageBox.Show(text, "DreamAmazon", MessageBoxButtons.OK, icon);
        }

        public SelectFileResult GetUserFileToSave(SelectFileOptions options)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = options.Title;
                sfd.FileName = options.FileName;
                sfd.Filter = options.Filter;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    return new SelectFileResult(sfd.FileName);
                }
            }

            return SelectFileResult.Empty;
        }

        public SelectFileResult GetUserFileToOpen(SelectFileOptions options)
        {
            using (OpenFileDialog sfd = new OpenFileDialog())
            {
                sfd.Title = options.Title;
                sfd.FileName = options.FileName;
                sfd.Filter = options.Filter;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    return new SelectFileResult(sfd.FileName);
                }
            }

            return SelectFileResult.Empty;
        }

        public virtual void ShowStatusMessage(string message)
        {
            
        }
    }
}