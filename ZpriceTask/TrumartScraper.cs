using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;



namespace ZpriceTask
{
    public static class TrumartScraper
    {
        private const string URL = "https://trumart.ru";

        //PAGE LOCATORS
        private const string TOKEN_INPUT_XPATH = "//div[contains(@class, 'items-setting')]//input[@name ='_token']";
        private const string ITEMS_INPUT_XPATH = "//div[contains(@class, 'items-setting')]//input[@name ='items']";
        private const string ITEM_CONTAINER_DIV_XPATH = "//div[contains(@class,'item-outside-long')]";

        //INSIDE ITEM LOCATORS
        private const string ITEM_PRICE_META_XPATH = ".//meta[@itemprop='price']";
        private const string ITEM_TITLE_DIV_XPATH = ".//div[contains(@class, 'title')]";
        private const string ITEM_CODE_DIV_XPATH = ".//div[contains(@class, 'code')]";

        public static List<Item> getItemsByVendor(string vendorName)
        {
            var res = new List<Item>();
            var Params = getPageParameters(vendorName);
            var page = 1;

            while (true)
            {
                try
                {
                    HtmlDocument htmlItemsPage = getAjaxItemsPage(vendorName, Params["token"], Params["items"], Params["cookies"], page);
                    var itemsList = getItemsFromHtml(htmlItemsPage);
                    res.AddRange(itemsList);
                    page++;
                }
                catch (Exception e) when (e.Message == "Page is empty")
                {
                    break;
                }
            }
            return res;
        }


        private static Dictionary<string, string> getPageParameters(string manufacturerName)
        {
            var html = URL + @"/vendor/" + manufacturerName;
            var res = new Dictionary<string, string>();

            CookieContainer _cookies = new CookieContainer();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(html);
                request.Method = "GET";
                request.CookieContainer = _cookies;

                HttpWebResponse responsee = (HttpWebResponse)request.GetResponse();
                var stream = responsee.GetResponseStream();
                using (var reader = new StreamReader(stream))
                {
                    string htmlTring = reader.ReadToEnd();
                    var document = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(htmlTring);
                }


            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;
            }

            var cookies = _cookies.GetCookies(new Uri(URL));
            var cookies_string = "";

            foreach (var cook in cookies)
            {
                cookies_string += cook.ToString() + "; ";
            }


            var token = doc.DocumentNode.SelectSingleNode(TOKEN_INPUT_XPATH).GetAttributeValue("value", "");
            var items = doc.DocumentNode.SelectSingleNode(ITEMS_INPUT_XPATH).GetAttributeValue("value", "");

            res.Add("cookies", cookies_string);
            res.Add("token", token);
            res.Add("items", items);

            return res;
        }




        private static HtmlDocument getAjaxItemsPage(string vendor, string token, string items, string cookies, int page)
        {

            var handler = new HttpClientHandler();
            handler.UseCookies = true;

            using (var httpClient = new HttpClient(handler))
            {
                var postrequest = (HttpWebRequest)WebRequest.Create("https://trumart.ru/ajax/cat/items");
                postrequest.Method = "POST";
                postrequest.KeepAlive = true;
                postrequest.Accept = "*/*";
                postrequest.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
                postrequest.Referer = "https://trumart.ru/vendor/POZIS";
                postrequest.Headers.Add("X-Requested-With", "XMLHttpRequest");
                postrequest.Headers.Add("Cookie", cookies);
                postrequest.ContentType = "application/x-www-form-urlencoded";

                StreamWriter requestWriter = new StreamWriter(postrequest.GetRequestStream());
                try
                {
                    requestWriter.Write($"id=&items={items.Replace(",", "%2C")}&" +
                        $"_token={token}&vendor={vendor}&row_char=&order=sort+ASC&available=false&novelty=false&discount=false&page={page}");
                }
                catch
                {
                    throw;
                }
                finally
                {
                    requestWriter.Close();
                    requestWriter = null;
                }

                try
                {
                    HttpWebResponse response = (HttpWebResponse)postrequest.GetResponse();
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    {
                        string responseString = sr.ReadToEnd();
                        var htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(responseString);
                        return htmlDoc;

                    }
                }
                catch (WebException wex)
                {
                    var pageContent = new StreamReader(wex.Response.GetResponseStream())
                                          .ReadToEnd();
                    Console.WriteLine(pageContent);
                    throw;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    throw;
                }
            }

        }

        private static List<Item> getItemsFromHtml(HtmlDocument page)
        {
            List<Item> res = new List<Item>();

            var itemBoxes = page.DocumentNode.SelectNodes(ITEM_CONTAINER_DIV_XPATH);

            if (itemBoxes == null)
            {
                throw new Exception("Page is empty");
            }

            foreach (var itemBox in itemBoxes)
            {

                string price = itemBox.SelectSingleNode(ITEM_PRICE_META_XPATH).Attributes["content"].Value;
                string priceFormated = Regex.Replace(price, "[^0-9]", "");
                string title = itemBox.SelectSingleNode(ITEM_TITLE_DIV_XPATH).InnerText.Trim();
                string code = itemBox.SelectSingleNode(ITEM_CODE_DIV_XPATH).InnerText.Trim();
                Item item = new Item(title, Decimal.Parse(priceFormated), int.Parse(code));

                res.Add(item);
            }

            return res;
        }
    }
}
