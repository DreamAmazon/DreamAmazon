using System.Collections.Generic;

namespace DreamAmazon.Interfaces
{
    public interface IProxyManager
    {
        Proxy GetProxy();
        Proxy QueueProxy(string ip, int port);
        Proxy QueueProxy(string ip, int port, string username, string pass);
        void RemoveProxy(Proxy proxy);
        void Clear();
        int Count { get; }
        IEnumerable<Proxy> Proxies { get; }
    }
}