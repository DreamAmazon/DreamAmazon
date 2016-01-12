using System;
using System.IO;

namespace DreamAmazon
{
    public static class UserAgentsManager
    {
        private static string[] _uAgents;

        static UserAgentsManager()
        {
            LoadAgents();
        }

        private static void LoadAgents()
        {
            if (File.Exists(@".\ua.txt"))
            {
                _uAgents = File.ReadAllLines(@".\ua.txt");
            }
            else
            {
                _uAgents = new string[1];
                _uAgents[0] = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.0.1) Gecko/2008070208 Firefox/3.0.1";
            }
        }

        public static string GetRandomUserAgent()
        {
            Random r = new Random();
            int index = r.Next(0, _uAgents.Length);

            return _uAgents[index];
        }
    }
}