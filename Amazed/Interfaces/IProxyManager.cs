using System.Net;

namespace DreamAmazon.Interfaces
{
    public interface IProxyManager
    {
        IWebProxy GetProxy();
        void QueueProxy(string ip, string port);
        void QueueProxy(string ip, string port, string username, string pass);
        void RemoveProxy(IWebProxy proxy);
        void Clear();
        int Count { get; }
    }
}