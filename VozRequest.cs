using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Net;

namespace TaiHinhVoz
{
    class VozRequest
    {
        public async Task<HttpClient> LogIn(string username, string password)
        {
            HttpClient httpClient = new HttpClient();
            var responseGetPage = await httpClient.GetAsync("https://next.voz.vn/");
            var contentResGetPage = await responseGetPage.Content.ReadAsStringAsync();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(contentResGetPage);

            HtmlNode node = doc.DocumentNode.SelectSingleNode("//html");

            string xfToken = node.Attributes["data-csrf"].Value;
            string urlLogin = "https://next.voz.vn/login/?_xfRequestUri=%2F&_xfWithData=1&_xfToken=" + xfToken + "&_xfResponseType=json";

            var responseGetFormLogin = await httpClient.GetAsync(urlLogin);
            var contentResGetFormLogin = await responseGetFormLogin.Content.ReadAsStringAsync();
            TypeFormLogin responseFormLogin = JsonConvert.DeserializeObject<TypeFormLogin>(contentResGetFormLogin);

            doc = new HtmlDocument();
            doc.LoadHtml(responseFormLogin.html.content);

            HtmlNode nodeTokenLogin = doc.DocumentNode.SelectSingleNode("//input[@name='_xfToken']");
            string _xfToken = nodeTokenLogin.Attributes["value"].Value;

            var dataPost = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string> ("login",username),
                new KeyValuePair<string, string> ("password",password),
                new KeyValuePair<string, string> ("remember", "1"),
                new KeyValuePair<string, string> ("_xfRedirect", "https://next.voz.vn/"),
                new KeyValuePair<string, string> ("_xfToken", _xfToken)
            }
            );

            var res = await httpClient.PostAsync("https://next.voz.vn/login/login", dataPost);
            var contentRes = await res.Content.ReadAsStringAsync();
            if (contentRes.Contains("Incorrect password"))
            {
                Console.WriteLine("Loi dang nhap");
                return null;
            }
            else
            {
                Console.WriteLine("Dang nhap thanh cong");
            }
            return httpClient;
        }
        public async Task DownloadImage(HttpClient httpClient, string url, int beginPage, int endPage)
        {
            Console.WriteLine("Dang get hinh thread {0}", url);
            //Find the last page
            var response = await httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(responseContent);

            HtmlNodeCollection nodePage = doc.DocumentNode.SelectNodes("//li[@class='pageNav-page ']");

            //If nodePage = null => only one page
            if (nodePage == null)
            {
                beginPage = 1;
                endPage = 1;
            }
            else
            {
                string lastPageString = nodePage[nodePage.Count - 1].InnerText;
                int lastPage = int.Parse(lastPageString);
                if (endPage > lastPage)
                {
                    endPage = lastPage;
                }
            }
            
            //Get link page & download
            string threadID = url.Replace("https://next.voz.vn", "").Split('.')[1];
            string localFolder = @"Hinh_" + threadID;
            Directory.CreateDirectory(localFolder);

            int count = 1;
            List<string> imgLink = new List<string>();
            for (int i = beginPage; i <= endPage; i++)
            {
                Console.WriteLine("Dang get hinh page {0}", i);
                Console.WriteLine("----------------------");
                string urlPage = "";
                HtmlDocument docPage = new HtmlDocument();
                if (i > 1)
                {
                    urlPage = url + "page-" + i;
                }
                else
                {
                    urlPage = url;
                }
                try
                {
                    var responsePage = await httpClient.GetAsync(urlPage);
                    var responsePageContent = await responsePage.Content.ReadAsStringAsync();
                    docPage.LoadHtml(responsePageContent);
                }
                catch (System.Net.Http.HttpRequestException)
                {
                    Console.WriteLine("Loi tai trang");
                }

                Thread.Sleep(2000);
                

                HtmlNodeCollection nodeImg = docPage.DocumentNode.SelectNodes("//img[contains(@src, 'data:image')]");

                if (nodeImg == null)
                {
                    continue;
                }

                Console.WriteLine("Da get xong hinh page {0}", i);
                Console.WriteLine("Bat dau tai hinh page {0}", i);
                Console.WriteLine("-----------------------");

                
                foreach (HtmlNode node in nodeImg)
                {
                    string directLinkImg = node.Attributes["data-src"].Value;
                    string localFilename = localFolder + @"\" + count + ".jpg";

                    try
                    {
                        var imageBytes = await httpClient.GetByteArrayAsync(directLinkImg);
                        File.WriteAllBytes(localFilename, imageBytes);
                        Console.WriteLine("Da tai xong hinh thu {0}", count);
                    }
                    catch(System.Net.Http.HttpRequestException)
                    {
                        Console.WriteLine("Loi tai hinh");
                    }
                    Thread.Sleep(1000);
                    count++;
                }

                Console.WriteLine("-----------------------");
                Console.WriteLine("Da tai xong hinh page {0}", i);
                Console.WriteLine("-----------------------");
            }
        }
    }
}
