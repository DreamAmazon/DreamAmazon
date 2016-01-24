using System.Net;

namespace DreamAmazon
{
    public static class ProxyHelper
    {
        public static WebProxy Create(Proxy proxy)
        {
            Contracts.Require(proxy != null);

            if (string.IsNullOrEmpty(proxy.UserName) && string.IsNullOrEmpty(proxy.Password))
            {
                return new WebProxy(proxy.IpAddress, proxy.Port) {UseDefaultCredentials = true};
            }
            return new WebProxy(proxy.IpAddress, proxy.Port)
            {
                Credentials = new NetworkCredential(proxy.UserName, proxy.Password)
            };
        }
    }
}