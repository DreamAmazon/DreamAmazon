using System;
using System.Collections.Generic;
using System.Net;
using DreamAmazon.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace DreamAmazon
{
    public class LoggedProxyManager : IProxyManager
    {
        private readonly IProxyManager _proxyManager;
        private readonly ILogger _logger;

        public int Count { get { return _proxyManager.Count; } }
        public IEnumerable<IWebProxy> Proxies { get { return _proxyManager.Proxies; } }

        public LoggedProxyManager(IProxyManager proxyManager)
        {
            if (proxyManager == null)
                throw new ArgumentNullException(nameof(proxyManager));

            _proxyManager = proxyManager;
            _logger = ServiceLocator.Current.GetInstance<ILogger>();
        }

        public IWebProxy GetProxy()
        {
            return _proxyManager.GetProxy();
        }

        public IWebProxy QueueProxy(string ip, int port)
        {
            try
            {
                return _proxyManager.QueueProxy(ip, port);
            }
            catch (Exception exception)
            {
                _logger.Debug("error while queue proxy : " + ip + ":" + port);
                _logger.Error(exception);
            }
            return null;
        }

        public IWebProxy QueueProxy(string ip, int port, string username, string pass)
        {
            return _proxyManager.QueueProxy(ip, port, username, pass);
        }

        public void RemoveProxy(IWebProxy proxy)
        {           
            try
            {
                _proxyManager.RemoveProxy(proxy);
            }
            catch (Exception exception)
            {
                _logger.Debug("error while remove proxy");
                _logger.Error(exception);
            }
        }

        public void Clear()
        {
            try
            {
                _proxyManager.Clear();
            }
            catch (Exception exception)
            {
                _logger.Debug("error while clear proxies");
                _logger.Error(exception);
            }
        }
    }
}