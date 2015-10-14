using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace IcarusChecker
{
    internal class Program
    {
        private static string fileName = "./icarus.dat";

        private static void Main(string[] args)
        {

            try
            {
                var username = args[0];
                var pwd = args[1];

                string str = LoginAndDownloadData(username, pwd);
                var idxS = str.IndexOf("<div id=\"tabs-4\">");
                var len = str.Substring(idxS).IndexOf("</div>");

                str = str.Substring(idxS, len);

                if (IsSameToFile(str))
                {
                    if (!File.Exists(fileName))
                    {
                        WriteToFile(str);
                        Console.WriteLine("File created!");

                    }
                    else
                        Console.WriteLine("Nothing important.");

                }
                else
                {
                    Console.WriteLine("New data!");
                    WriteToFile(str);

                }



            }
            catch (IndexOutOfRangeException)
            {

                Console.WriteLine("usage: IcarusChecker.exe <username> <password>");
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine("Possibly wrong password entered.");
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine(e.StackTrace);

            }
            catch (Exception e)
            {
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine("Error getting data.");
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine(e.StackTrace);
            }


        }

        private static string LoginAndDownloadData(string username, string pwd)
        {
            string pBody = $"username={username}&pwd={pwd}";
            var loginAddress = "https://icarus-icsd.aegean.gr/authentication.php";
            var cookies = new CookieContainer();
            HttpWebRequest req = (HttpWebRequest) WebRequest.Create(loginAddress);
            req.CookieContainer = cookies;
            req.ContentLength = 33;
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            var stream = req.GetRequestStream();
            stream.Write(Encoding.UTF8.GetBytes(pBody), 0,
                pBody.Length);

            var resp = (HttpWebResponse) req.GetResponse();

            using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.Default))
            {
                return sr.ReadToEnd();
            }
        }

        private static bool IsSameToFile(string newContent)
        {
            if (File.Exists(fileName))
            {
                if (newContent == File.ReadAllText(fileName))
                    return true;
                return false;
            }
            return true;

        }

        private static void WriteToFile(string content)
        {

            File.AppendAllText(fileName, content);
        }
    }

}
