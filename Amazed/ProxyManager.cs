using System;
using System.Collections.Generic;
using System.Net;
using DreamAmazon.Interfaces;

namespace DreamAmazon
{
    public class ProxyManager : IProxyManager
    {
        private readonly List<IWebProxy> _proxies = new List<IWebProxy>();

        public int Count => _proxies.Count;

        public IWebProxy GetProxy()
        {
            if (_proxies.Count != 0)
            {
                Random rand = new Random();
                return _proxies[rand.Next(0, _proxies.Count)];
            }
            return null;
        }

        public void QueueProxy(string ip, string port)
        {
            IWebProxy proxy = new WebProxy(ip, Convert.ToInt32(port));
            _proxies.Add(proxy);
        }

        public void QueueProxy(string ip, string port, string username, string pass)
        {
            IWebProxy proxy = new WebProxy(ip, Convert.ToInt32(port));
            proxy.Credentials = new NetworkCredential(username, pass);
            _proxies.Add(proxy);
        }

        public void RemoveProxy(IWebProxy proxy)
        {
            if (proxy == null)
                return;

            if (_proxies.Contains(proxy))
                _proxies.Remove(proxy);
        }

        public void Clear()
        {
            _proxies.Clear();
        }
    }
}