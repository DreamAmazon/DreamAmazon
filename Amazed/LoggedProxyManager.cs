using System;
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

        public void QueueProxy(string ip, string port)
        {
            _proxyManager.QueueProxy(ip, port);
        }

        public void QueueProxy(string ip, string port, string username, string pass)
        {
            _proxyManager.QueueProxy(ip, port, username, pass);
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