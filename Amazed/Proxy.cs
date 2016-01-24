namespace DreamAmazon
{
    public class Proxy
    {
        public static readonly Proxy Empty = new Proxy();

        public string IpAddress { get; }
        public int Port { get; }
        public string UserName { get; }
        public string Password { get; }

        protected Proxy()
        { }

        public Proxy(string ip, int port)
        {
            Contracts.Require(!string.IsNullOrEmpty(ip));
            Contracts.Require(port > 9);

            IpAddress = ip;
            Port = port;
        }

        public Proxy(string ip, int port, string user, string password)
        {
            Contracts.Require(!string.IsNullOrEmpty(ip));
            Contracts.Require(port > 9);
            Contracts.Require(!string.IsNullOrEmpty(user));
            Contracts.Require(!string.IsNullOrEmpty(password));

            IpAddress = ip;
            Port = port;
            UserName = user;
            Password = password;
        }
    }
}