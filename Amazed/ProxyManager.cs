using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using DreamAmazon.Interfaces;

namespace DreamAmazon
{
    public class ProxyManager : IProxyManager
    {
        private readonly ConcurrentDictionary<IWebProxy, object> _proxies = new ConcurrentDictionary<IWebProxy, object>();

        public int Count => _proxies.Count;
        public IEnumerable<IWebProxy> Proxies { get { return _proxies.Keys; } }

        private readonly IWebProxy _defaultProxy;

        public ProxyManager()
        {
            _defaultProxy = WebRequest.GetSystemWebProxy();
            _defaultProxy.Credentials = CredentialCache.DefaultNetworkCredentials;
        }

        public IWebProxy GetProxy()
        {
            if (_proxies.Count == 0) return _defaultProxy;

            Random rand = new Random();
            var index = rand.Next(0, _proxies.Count - 1);

            int i = 0;
            foreach (var proxy in _proxies.Keys)
            {
                if (i == index)
                    return proxy;
                i++;
            }
            return _defaultProxy;
        }

        public IWebProxy QueueProxy(string ip, int port)
        {
            IWebProxy proxy = new WebProxy(ip, port) {UseDefaultCredentials = true};
            _proxies.TryAdd(proxy, null);
            return proxy;
        }

        public IWebProxy QueueProxy(string ip, int port, string username, string pass)
        {
            IWebProxy proxy = new WebProxy(ip, port);
            proxy.Credentials = new NetworkCredential(username, pass);
            _proxies.TryAdd(proxy, null);
            return proxy;
        }

        public void RemoveProxy(IWebProxy proxy)
        {
            if (proxy == null)
                return;

            object o;
            if (_proxies.ContainsKey(proxy))
                _proxies.TryRemove(proxy, out o);
        }

        public void Clear()
        {
            _proxies.Clear();
        }
    }
}