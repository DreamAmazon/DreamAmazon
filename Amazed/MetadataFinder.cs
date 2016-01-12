using System;
using System.IO;
using System.Threading;

namespace DreamAmazon
{
    class MetadataFinder
    {
        private readonly Account _account;
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        private string _metadata;

        public MetadataFinder(Account account)
        {
            _account = account;
        }

        public string Find(TimeSpan timeSpan)
        {
            _metadata = null;
            var tokenThread = new Thread(() =>
            {
                GetAccountToken(_account);
            });
            tokenThread.SetApartmentState(ApartmentState.STA);
            tokenThread.Start();

            if (!_manualResetEvent.WaitOne(timeSpan))
            {
                return null;
            }

            return _metadata;
        }

        private void GetAccountToken(Account account)
        {
            using (System.Windows.Forms.WebBrowser wb = new System.Windows.Forms.WebBrowser())
            {
                wb.DocumentCompleted += wb_DocumentCompleted;
                wb.Tag = account;
                wb.ScriptErrorsSuppressed = true;
                wb.Url = new Uri(string.Format("file:///{0}", Path.GetFullPath("md.html")));

                System.Windows.Forms.Application.Run();
            }
        }

        private void wb_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e)
        {
            System.Windows.Forms.WebBrowser wb = (System.Windows.Forms.WebBrowser)sender;

            if (wb.Document != null)
            {
                var metadataToken = wb.Document.Body.InnerText.Replace("\r\n", "");
                _metadata = metadataToken;
            }
            _manualResetEvent.Set();

            System.Windows.Forms.Application.ExitThread();
        }
    }
}