using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DreamAmazon.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace DreamAmazon
{
    public class MetadataFinder : IDisposable
    {
        private static volatile MetadataFinder _instance;
        private static readonly object SyncRoot = new object();

        private bool _breakWaitResponse;
        bool _breakProcessQueue;
        private readonly ILogger _logger;

        private readonly Uri _mdUrl = new Uri(string.Format("file:///{0}", Path.GetFullPath("md.html")));

        private System.Windows.Forms.WebBrowser _webBrowser;

        private readonly ConcurrentQueue<Account> _queries = new ConcurrentQueue<Account>();
        private readonly ConcurrentDictionary<Account, string> _responses = new ConcurrentDictionary<Account, string>();

        public static MetadataFinder Create()
        {
            if (_instance == null)
            {
                lock (SyncRoot)
                {
                    if (_instance == null)
                        _instance = new MetadataFinder();
                }
            }

            return _instance;
        }

        protected MetadataFinder()
        {
            _logger = ServiceLocator.Current.GetInstance<ILogger>();

            StartBrowserInSTA();
            StartProcessQueue();
        }

        private void StartProcessQueue()
        {
            Task.Factory.StartNew(ProcessQueries);
        }

        private void StartBrowserInSTA()
        {
            var tokenThread = new Thread(StartBrowser) { IsBackground = true };
            tokenThread.SetApartmentState(ApartmentState.STA);
            tokenThread.Start();
        }

        private void StartBrowser()
        {
            _webBrowser = new System.Windows.Forms.WebBrowser();
            _webBrowser.DocumentCompleted += WebBrowserDocumentCompleted;
            _webBrowser.ScriptErrorsSuppressed = true;
            _webBrowser.Url = _mdUrl;
            System.Windows.Forms.Application.Run();
        }

        private void ProcessQueries()
        {
            // never returns
            while (!_breakProcessQueue)
            {
                _logger.Debug("waiting new queries");

                if (_queries.Count == 0)
                    Thread.Sleep(1000);

                Account account;
                while (!_queries.TryDequeue(out account))
                {
                    Thread.Sleep(500);
                    _logger.Debug("can't dequeue " + account.Email);
                }

                _logger.Debug("query found " + account.Email);
                _webBrowser.Tag = account;
                _webBrowser.Navigate(_mdUrl);
            }
        }

        public string QueryMetadata(Account account)
        {
            _queries.Enqueue(account);

            while (!_breakWaitResponse && !_responses.ContainsKey(account))
            {
                Thread.Sleep(1000);
                _logger.Debug("waiting response " + account.Email);
            }

            if (_breakWaitResponse)
            {
                return null;
            }

            string metadata;
            while(!_responses.TryRemove(account, out metadata))
            {
                Thread.Sleep(500);
                _logger.Debug("can't remove response " + account.Email);
            }
            _logger.Debug("response removed " + account.Email);
            return metadata;
        }

        private void WebBrowserDocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e)
        {
            _logger.Debug("document loaded");

            System.Windows.Forms.WebBrowser browser = (System.Windows.Forms.WebBrowser)sender;

            var account = browser.Tag as Account;

            if (account == null)
                _logger.Debug("browser tag is null");

            _logger.Debug("document loaded " + account.Email);

            if (browser.Document != null)
            {
                var metadataToken = browser.Document.Body.InnerText.Replace("\r\n", "");
                _logger.Debug("document loaded " + account.Email + " metadata=" + metadataToken);

                while (!_responses.TryAdd(account, metadataToken))
                {
                    Thread.Sleep(500);
                    _logger.Debug("can't add response " + account.Email);
                }
                _logger.Debug("response added " + account.Email);
            }
            else
            {
                _logger.Debug("empty document loaded " + account.Email);
            }
        }

        public void Dispose()
        {
            _breakProcessQueue = true;
            _breakWaitResponse = true;
            _webBrowser.DocumentCompleted -= WebBrowserDocumentCompleted;
            _webBrowser.Dispose();
            _webBrowser = null;
        }
    }
}