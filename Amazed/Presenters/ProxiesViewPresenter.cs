using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DreamAmazon.Events;
using DreamAmazon.Interfaces;
using DreamAmazon.Models;
using EventAggregatorNet;
using Microsoft.Practices.ServiceLocation;

namespace DreamAmazon.Presenters
{
    public class ProxiesViewPresenter : BasePresenter
    {
        private readonly IProxiesView _view;
        private readonly IProxyManager _proxyManager;
        private readonly SettingModel _setting;
        private readonly IEventAggregator _eventAggregator;

        public ProxiesViewPresenter(IProxiesView view)
        {
            Contracts.Require(view != null);

            _view = view;

            _view.ImportProxiesRequested += View_ImportProxiesRequested;
            _view.ClearProxiesRequested += View_ClearProxiesRequested;
            _view.TestProxiesRequested += View_TestProxiesRequested;

            _proxyManager = ServiceLocator.Current.GetInstance<IProxyManager>();
            _setting = ServiceLocator.Current.GetInstance<ISettingsService>().GetSettings();
            _eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
        }

        private async void View_TestProxiesRequested()
        {
            _view.EnableTestProxiesRequest(false);
            await Test();
            DisplayProxies();

            _eventAggregator.SendMessage(new ProxiesStatisticMessage(GetProxiesStatistic()));

            _view.EnableTestProxiesRequest(true);
        }

        private Task Test()
        {
            return Task.Factory.StartNew(() =>
            {
                var dead = new ConcurrentStack<Proxy>();
                Parallel.ForEach(_proxyManager.Proxies, proxy =>
                {
                    bool result = false;
                    var mre = new ManualResetEvent(false);
                    Thread thread = new Thread(() =>
                    {
                        result = NetHelper.TestProxy1(ProxyHelper.Create(proxy));
                        mre.Set();
                    }) {IsBackground = true};
                    thread.Start();

                    if (!mre.WaitOne(TimeSpan.FromSeconds(10)))
                    {
                        dead.Push(proxy);
                        thread.Abort();
                    }
                    else
                    {
                        if (!result)
                        {
                            dead.Push(proxy);
                        }
                    }
                });

                while (dead.Count > 0)
                {
                    Proxy proxy;
                    if (dead.TryPop(out proxy))
                    {
                        _proxyManager.RemoveProxy(proxy);
                    }
                }
            });
        }

        private void View_ClearProxiesRequested()
        {
            _proxyManager.Clear();
            DisplayProxies();
            _eventAggregator.SendMessage(new ProxiesStatisticMessage(GetProxiesStatistic()));
        }

        private void View_ImportProxiesRequested()
        {
            LoadProxies();
        }

        public void Start()
        {
            if (IsViewActive("frmProxies"))
            {
                _view.Show();
                DisplayProxies();
            }
        }

        private void LoadProxies()
        {
            var selectedFile = _view.GetUserFileToOpen(new SelectFileOptions
            {
                Title = "Choose a file containing a list of proxies...",
                Filter = "Text Files (*.txt)|*.txt"
            });

            if (selectedFile == SelectFileResult.Empty)
                return;

            _proxyManager.Clear();

            _view.EnableImportProxiesRequested(false);

            Parallel.ForEach(File.ReadAllLines(selectedFile.FileName), line =>
            {
                if (!line.Contains(":")) return;

                var data = line.Split(':');

                if (_setting.UseStandardProxies)
                {
                    if (data.Length >= 2)
                    {
                        int port;
                        if (int.TryParse(data[1], out port))
                        {
                            _proxyManager.QueueProxy(data[0], port);
                        }
                    }
                }
                else if (data.Length >= 4)
                {
                    int port;
                    if (int.TryParse(data[1], out port))
                    {
                        _proxyManager.QueueProxy(data[0], port, data[2], data[3]);
                    }
                }
            });

            _view.EnableImportProxiesRequested(true);
            DisplayProxies();

            _eventAggregator.SendMessage(new ProxiesStatisticMessage(GetProxiesStatistic()));
        }

        private ProxiesStatistic GetProxiesStatistic()
        {
            return new ProxiesStatistic
            {
                Total = _proxyManager.Count
            };
        }

        private void DisplayProxies()
        {
            _view.ClearProxies();
            foreach (var proxy in _proxyManager.Proxies)
            {
                _view.DisplayProxy(proxy);
            }
        }
    }
}