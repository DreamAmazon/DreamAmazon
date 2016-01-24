using System.Collections.Generic;
using System.Net;

namespace DreamAmazon.Interfaces
{
    public interface IProxyManager
    {
        IWebProxy GetProxy();
        IWebProxy QueueProxy(string ip, int port);
        IWebProxy QueueProxy(string ip, int port, string username, string pass);
        void RemoveProxy(IWebProxy proxy);
        void Clear();
        int Count { get; }
        IEnumerable<IWebProxy> Proxies { get; }
    }
}