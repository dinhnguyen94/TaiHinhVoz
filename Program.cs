using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

namespace TaiHinhVoz
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Tool download hinh anh tren NextvOz by talaai1312_ver2");
            Console.WriteLine("-------------------------");

            VozRequest vozRequest = new VozRequest();
            List<string> urlImg = File.ReadAllLines("link.txt").ToList();
            string infoAccount = File.ReadAllText("account.txt");
            string[] splitAccount = infoAccount.Split('|');
            string username = splitAccount[0];
            string password = splitAccount[1];

            //Log in
            HttpClient httpClient = new HttpClient();
            if (username == "" || password == "")
            {
                Console.WriteLine("Vui long nhap user/pass");
                Console.ReadKey();
                return;
            }
            try
            {
                httpClient = vozRequest.LogIn(username, password).GetAwaiter().GetResult();
            }
            catch(System.Net.Http.HttpRequestException)
            {
                Console.WriteLine("Loi gi do khong biet");
            }

            if (httpClient == null)
            {
                Console.ReadKey();
                return;
            }

            //Download image
            foreach(string request in urlImg)
            {
                string[] splitRequest = request.Split('|');
                string url = splitRequest[0];
                bool beginPageParse = int.TryParse(splitRequest[1], out int beginPage);
                bool endPageParse = int.TryParse(splitRequest[2], out int endPage);
                if (url == "" || !beginPageParse || !endPageParse || (beginPage > endPage))
                {
                    Console.WriteLine("Loi dinh dang url, vui long dinh dang theo dung huong dan");
                    continue;
                }

                if (beginPage == 0)
                {
                    beginPage = 1;
                }
                try
                {
                    vozRequest.DownloadImage(httpClient, url, beginPage, endPage).GetAwaiter().GetResult();
                }
                catch (System.Net.Http.HttpRequestException)
                {
                    Console.WriteLine("Loi gi do khong biet");
                }
            }

            Console.WriteLine("Da tai xong het tat ca cac hinh");
            Console.ReadKey();
        }
    }
}
