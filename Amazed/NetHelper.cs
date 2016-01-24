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
        public CookieContainer Cookies { get; protected set; }

        public string UserAgent { get; set; }

        public IWebProxy Proxy { get; set; }

        public NetHelper()
        {
            Cookies = new CookieContainer();
        }

        public Result<string> POST(string url, Dictionary<string, string> postParameters)
        {
            string postData = string.Empty;

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

            if (Proxy != null)
            {
                request.Proxy = Proxy;
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
                            var content = sr.ReadToEnd();
                            return Result.Ok(content);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                return Result.Fail<string>(exception.Message);
            }    
            
        }

        public Result<string> GET(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";

            request.CookieContainer = Cookies;
            request.KeepAlive = true;
            request.ContentType = "application/x-www-form-urlencoded; text/plain; charset=UTF-8";
            request.AllowAutoRedirect = true;
            request.UserAgent = UserAgent;
            request.Accept = "*/*";

            if (Proxy != null)
            {
                request.Proxy = Proxy;
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
                            var content = sr.ReadToEnd();
                            return Result.Ok(content);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                return Result.Fail<string>(exception.Message);
            }
        }

        public static bool TestRequestConnect(IWebProxy proxy)
        {
            bool isOk = false;
            try
            {
                HttpWebRequest request = HttpWebRequest.Create(new Uri("https://www.amazon.com/")) as HttpWebRequest;
                request.KeepAlive = true;
                //request.Credentials = CredentialCache.DefaultCredentials;
                request.Proxy = proxy;
                //request.Timeout = 5000;
                var wresp = (HttpWebResponse)request.GetResponse();
                string line;
                using (var reader = new StreamReader(wresp.GetResponseStream()))
                {
                    line = reader.ReadToEnd();
                }

                isOk = !string.IsNullOrWhiteSpace(line);
            }
            catch (Exception)
            {

            }
            return isOk;
        }
    }
}
