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

        private bool _breakWaitNavigation;
        private bool _breakWaitResponse;
        private bool _breakProcessQueue;
        private readonly ILogger _logger;

        private readonly Uri _mdUrl = new Uri(string.Format("file:///{0}", Path.GetFullPath("md.html")));

        private System.Windows.Forms.WebBrowser _webBrowser;

        private Account _navigationCompleteAccount;

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
            //_webBrowser.Url = _mdUrl;
            System.Windows.Forms.Application.Run();
        }

        private void ProcessQueries()
        {
            // never returns
            while (!_breakProcessQueue)
            {
                WaitQueries();

                Account account;
                while (!_queries.TryDequeue(out account))
                {
                    Thread.Sleep(500);
                }

                _webBrowser.Tag = account;
                _webBrowser.Navigate(_mdUrl);

                WaitBrowserNavigation(account);
            }
        }

        private void SetBrowserNavigationComplete(Account account)
        {
            _navigationCompleteAccount = account;
        }

        private void WaitBrowserNavigation(Account account)
        {
            while (!_breakWaitNavigation && _navigationCompleteAccount != account)
            {
                Thread.Sleep(1000);
            }

            _navigationCompleteAccount = null;

            if (_breakWaitNavigation)
            {
                throw new OperationCanceledException();
            }
        }

        private void WaitResponseReady(Account account)
        {
            while (!_breakWaitResponse && !IsResponseExist(account))
            {
                Thread.Sleep(1000);
            }

            if (_breakWaitResponse)
            {
                throw new OperationCanceledException();
            }
        }

        private void WaitQueries()
        {
            if (_queries.Count == 0)
            {
                Thread.Sleep(1000);
            }
        }

        private bool IsResponseExist(Account account)
        {
            return _responses.ContainsKey(account);
        }

        public string QueryMetadata(Account account)
        {
            EnqueueAccount(account);

            WaitResponseReady(account);

            string metadata;
            while(!_responses.TryRemove(account, out metadata))
            {
                Thread.Sleep(500);
            }
            return metadata;
        }

        private void EnqueueAccount(Account account)
        {
            _queries.Enqueue(account);
        }

        private void WebBrowserDocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e)
        {
            System.Windows.Forms.WebBrowser browser = (System.Windows.Forms.WebBrowser)sender;

            var account = browser.Tag as Account;

            Contracts.Require(account != null);

            if (browser.Document != null)
            {
                var metadataToken = browser.Document.Body.InnerText.Replace("\r\n", "");

                while (!_responses.TryAdd(account, metadataToken))
                {
                    Thread.Sleep(500);
                }
                SetBrowserNavigationComplete(account);
            }
            else
            {
                SetBrowserNavigationComplete(account);
                EnqueueAccount(account);
            }
        }

        private void BreakOperations()
        {
            _breakProcessQueue = true;
            _breakWaitResponse = true;
            _breakWaitNavigation = true;
        }

        public void Dispose()
        {
            BreakOperations();
            _webBrowser.DocumentCompleted -= WebBrowserDocumentCompleted;
            _webBrowser.Dispose();
            _webBrowser = null;
        }
    }
}