using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace DreamAmazon
{
    public class NetHelper
    {
        private CookieContainer _cookies;
        public CookieContainer Cookies
        {
            get { return _cookies; }
            protected set { _cookies = value; }
        }

        private String _ua;
        public String UserAgent
        {
            get { return _ua; }
            set { _ua = value; }
        }        

        private IWebProxy proxy;
        public IWebProxy Proxy
        {
            get { return proxy; }
            set { proxy = value; }
        }
        
        public NetHelper()
        {
            this.Cookies = new CookieContainer();
        }

        public string POST(string url, Dictionary<string, string> postParameters)
        {
            string postData = "";

            foreach (string key in postParameters.Keys)
            {
                postData += HttpUtility.UrlEncode(key) + "=" + HttpUtility.UrlEncode(postParameters[key]) + "&";
            }

            postData = postData.TrimEnd('&');

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";

            byte[] data = Encoding.ASCII.GetBytes(postData);

            request.CookieContainer = Cookies;
            request.KeepAlive = true;
            request.ContentType = "application/x-www-form-urlencoded; text/plain; charset=UTF-8";
            request.ContentLength = data.Length;
            request.AllowAutoRedirect = true;
            request.UserAgent = UserAgent;
            request.Accept = "*/*";

            if (proxy != null)
            {
                request.Proxy = proxy;
            }

            try
            {
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(data, 0, data.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    foreach (Cookie c in response.Cookies)
                        Cookies.Add(c);

                    using (Stream responseStream = response.GetResponseStream())
                    {
                        if (responseStream == null)
                        {
                            throw new ArgumentNullException();
                        }
                        using (StreamReader sr = new StreamReader(responseStream, Encoding.Default))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return "NetError";
            }    
            
        }

        public string GET(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";

            request.CookieContainer = this.Cookies;
            request.KeepAlive = true;
            request.ContentType = "application/x-www-form-urlencoded; text/plain; charset=UTF-8";
            request.AllowAutoRedirect = true;
            request.UserAgent = UserAgent;
            request.Accept = "*/*";

            if (proxy != null)
            {
                request.Proxy = proxy;
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    var code = response.StatusCode;

                    foreach (Cookie c in response.Cookies)
                        Cookies.Add(c);

                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(responseStream, Encoding.Default))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return "NetError";
            }
        }
    }
}
