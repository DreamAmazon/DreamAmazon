using System;

namespace DreamAmazon
{
    public class Globals
    {
        public static string LicensedName;
        public static string LicensedEmail;
        public static string REGEX;
        public static string LOG_URL;
        public static string POST_URL;
        public static string[] BADLOG_MSG;
        public static string CAPTCHA_MSG;
        public static string GC_URL;
        public static string ZIP_URL;
        public static string ORDERS_URL;
        public static string GC_REGEX;
        public static string ORDERS_REGEX;
        public static string ADDY_URL;
        public static string FULLADDY_URL;
        public static string CaptchaUrlRegex;

        public static bool Process(string key, string data) //Will be used when you setup your HWID
        {
            string[] split = data.Split(new [] { "|Spot|" }, StringSplitOptions.None);
            if (split.Length == 14 && split[0] == key)
            {
                LicensedName = split[1];
                LicensedEmail = split[2];
                REGEX = split[3].Replace(">input", "<input").Replace("/<", "/>");
                LOG_URL = @"https://www.amazon.com/ap/signin?openid.pape.max_auth_age=5400&openid.return_to=https%3A%2F%2Ffresh.amazon.com%2F%3FredirectedAfterSignIn%3Dtrue&openid.identity=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0%2Fidentifier_select&openid.assoc_handle=amazonfresh&openid.mode=checkid_setup&disableCorpSignUp=1&marketPlaceId=ATVPDKIKX0DER&authCookies=1&openid.claimed_id=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0%2Fidentifier_select&pageId=amazonfresh&openid.ns=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0&";
                POST_URL = split[5];
                BADLOG_MSG = new[]
                {
                    "There was an error with your E-Mail/ Password combination. Please try again.",
                    "Your email or password was incorrect. Please try again."
                };
                CAPTCHA_MSG = split[7];
                GC_URL = split[8];
                GC_REGEX = split[9];
                ORDERS_URL = split[10];
                ORDERS_REGEX = split[11];
                ADDY_URL = split[12];
                FULLADDY_URL = split[13].Replace("\0", "");
                CaptchaUrlRegex = "\"http.+captcha.+\\.jpg[^\"]*\"";

                return true;
            }
            return false;
        }

        public static void ProcessBypass()
        {
            REGEX = "<input.*? type=\"hidden\".name=\"([^\"]*?)\".*?value=\"([^\"]*?)\" />";
            LOG_URL = @"https://www.amazon.com/ap/signin?openid.pape.max_auth_age=5400&openid.return_to=https%3A%2F%2Ffresh.amazon.com%2F%3FredirectedAfterSignIn%3Dtrue&openid.identity=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0%2Fidentifier_select&openid.assoc_handle=amazonfresh&openid.mode=checkid_setup&disableCorpSignUp=1&marketPlaceId=ATVPDKIKX0DER&authCookies=1&openid.claimed_id=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0%2Fidentifier_select&pageId=amazonfresh&openid.ns=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0&";
            POST_URL = @"https://www.amazon.com/ap/signin";
            BADLOG_MSG = new[]
            {
                "There was an error with your E-Mail/ Password combination. Please try again.",
                "Your email or password was incorrect. Please try again."
            };
            CAPTCHA_MSG = "Type the characters you see in this image";
            GC_URL = @"https://www.amazon.com/gp/css/gc/balance/ref=gc_ya";
            GC_REGEX = @"(\\$[\\d\\.]+)";
            ORDERS_URL = @"https://www.amazon.com/gp/your-account/order-history/ref=oh_aui_no_orders_to_orders_by_date?ie=UTF8&digitalOrders=1&opt=ab&orderFilter=year-2015&returnTo=&unifiedOrders=1";
            ORDERS_REGEX = "<span class=\"num-orders\">(.*)</span>";
            ADDY_URL = @"https://www.amazon.com/gp/css/account/address/view.html?";
            FULLADDY_URL = @"https://www.amazon.com/gp/css/account/address/view.html?ie=UTF8&addressID={0}&ref_=myab_view_edit_address_form&viewID=editAddress";
            CaptchaUrlRegex = "\"http.+captcha.+\\.jpg[^\"]*\"";
        }
    }
}