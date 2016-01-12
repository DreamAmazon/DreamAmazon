using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DreamAmazon
{
    class License
    {
        public static String VERSION = "1.0";

        public static bool Init()
        {
            using (WebClient wc = new WebClient())
            {
                string key = Properties.Settings.Default.LicenseKey;
                string output = wc.DownloadString(@"http://dreamzje.me/DreamAmzn/amazing.php?key=" + key + "&id=" + HWID.Generate());
                string decrypted = DecryptData(output, ReverseString(key));
                
                return Globals.Process(key, decrypted);
            }
        }

        public static async Task<bool> InitAsync()
        {
            using (WebClient wc = new WebClient())
            {
                string key = Properties.Settings.Default.LicenseKey;
                string output = await wc.DownloadStringTaskAsync(@"http://dreamzje.me/DreamAmzn/amazing.php?key=" + key + "&id=" + HWID.Generate());
                string decrypted = DecryptData(output, ReverseString(key));
                
                return Globals.Process(key, decrypted);
            }
        }

        public static String GetMD5()
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(System.Reflection.Assembly.GetExecutingAssembly().Location))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }

        private static String DecryptData(string text, string key)
        {
            Encoding gEnc = Encoding.GetEncoding(1252);

            RijndaelManaged aes = new RijndaelManaged();
            aes.KeySize = 256;
            aes.BlockSize = 256;
            aes.Padding = PaddingMode.Zeros;
            aes.Mode = CipherMode.CBC;

            aes.Key = gEnc.GetBytes(key);

            text = gEnc.GetString(Convert.FromBase64String(text));

            string IV = text;
            IV = IV.Substring(IV.IndexOf("-[--IV-[-") + 9);
            text = text.Replace("-[--IV-[-" + IV, "");                     

            text = Convert.ToBase64String(Encoding.GetEncoding(1252).GetBytes(text));
            aes.IV = gEnc.GetBytes(IV);

            ICryptoTransform AESDecrypt = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] buffer = Convert.FromBase64String(text);

            return gEnc.GetString(AESDecrypt.TransformFinalBlock(buffer, 0, buffer.Length));
        }

        private static String ReverseString(String str)
        {
            string output = "";
            for (int i = str.Length - 1; i >= 0; i--)
            {
                output += str[i];
            }

            return output;
        }
    }
}
