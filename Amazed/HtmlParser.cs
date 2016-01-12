using HtmlAgilityPack;

namespace DreamAmazon
{
    public static class HtmlParser
    {
        public static string GetElementValueById(string html, string id)
        {
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                var el = doc.GetElementbyId(id);
                if (el == null)
                {
                    return null;
                }
                return el.GetAttributeValue("value", null);
            }
            catch
            {
                return null;
            }
        }
    }
}