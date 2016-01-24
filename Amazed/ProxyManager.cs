using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DreamAmazon.Interfaces;

namespace DreamAmazon
{
    public class ProxyManager : IProxyManager
    {
        private readonly ConcurrentDictionary<Proxy, object> _proxies = new ConcurrentDictionary<Proxy, object>();

        public int Count => _proxies.Count;
        public IEnumerable<Proxy> Proxies { get { return _proxies.Keys; } }

        private readonly Proxy _defaultProxy = Proxy.Empty;

        public Proxy GetProxy()
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

        public Proxy QueueProxy(string ip, int port)
        {
            Proxy proxy = new Proxy(ip, port);
            _proxies.TryAdd(proxy, null);
            return proxy;
        }

        public Proxy QueueProxy(string ip, int port, string username, string pass)
        {
            Proxy proxy = new Proxy(ip, port, username, pass);
            _proxies.TryAdd(proxy, null);
            return proxy;
        }

        public void RemoveProxy(Proxy proxy)
        {
            if (proxy == null || proxy == Proxy.Empty)
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