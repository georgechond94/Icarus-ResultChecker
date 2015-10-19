/*
    Icarus Result Checker 
    Copyright (C) 2015  Icarus Checker

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    Author: Chondrompilas Georgios

*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace IcarusChecker
{
    internal class Program
    {
        private readonly static string fileName = "./icarus.dat";
        private readonly static string emailUsername = "icaruschecker@zoho.com";
        private readonly static string emailPwd = "icarus@icsd";
        private readonly static string outEmailServer = "smtp.zoho.com";
        private readonly static int outEmailPort = 587;

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr zeroOnly, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private static void Main(string[] args)
        {

            try
            {
                var username = args[0];
                var pwd = args[1];
                Console.WriteLine("\nStarting.. Press Ctrl+C to exit.");

                while (true)
                {
                    
                    string str = LoginAndDownloadData(username, pwd);
                    var idxS = str.IndexOf("<div id=\"tabs-4\">", StringComparison.Ordinal);
                    var len = str.Substring(idxS).IndexOf("</div>", StringComparison.Ordinal);

                    str = str.Substring(idxS, len);
                    var same = IsSameToFile(str);
                    if (same==true)
                    {
                       
                            Console.WriteLine("Nothing important.");

                    }
                    else if (same==null)
                    {
                        WriteToFile(str);
                        Console.WriteLine("File created!");
                    }
                    else
                    {
                        Console.WriteLine("New data! Check your Icarus result table!");
                        WriteToFile(str);
                        NotifyUser();
                        SendEmail(username);


                    }
                    Thread.Sleep(60000);
                }


            }
            catch (IndexOutOfRangeException)
            {

                Console.WriteLine("usage: IcarusChecker.exe <username> <password>");
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine("Possibly wrong password entered.");
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine(e.Message);

            }
            catch (Exception e)
            {
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine("Error getting data.");
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine(e.Message);
            }


        }

        private static string LoginAndDownloadData(string username, string pwd)
        {
            string pBody = $"username={username}&pwd={pwd}";
            var loginAddress = "https://icarus-icsd.aegean.gr/authentication.php";
            var cookies = new CookieContainer();
            HttpWebRequest req = (HttpWebRequest) WebRequest.Create(loginAddress);
            req.CookieContainer = cookies;
            req.ContentLength = pBody.Length;
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

        private static bool? IsSameToFile(string newContent)
        {
            if (File.Exists(fileName))
            {
                if (String.CompareOrdinal(newContent,File.ReadAllText(fileName))==0)
                    return true;
                return false;
            }
            return null;

        }

        private static void WriteToFile(string content)
        {

            File.WriteAllText(fileName, content);
        }

        private static void NotifyUser()
        {
            SetForegroundWindow(FindWindowByCaption(IntPtr.Zero, Console.Title));
            Console.Beep(10000,1);
        }

        private static object tok = new object();

        private static void SendEmail(string username)
        {
            SmtpClient client = new SmtpClient(outEmailServer, outEmailPort);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(emailUsername, emailPwd);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Timeout = 60 * 1000 * 5;
            client.SendAsyncCancel();
            client.SendAsync(emailUsername, username + "@icsd.aegean.gr", "Result announced - Icarus Result Checker", "Hello " + username + ",\nGood/Bad news! I just wanted you to know that an exam result was announced just moments ago! Good luck!\n\nThis is an automated email. Do not reply.\nIcarus Result Checker",tok);
            client.SendCompleted += (sender, args) => {
                Console.WriteLine("Email sent to " + username + "@icsd.aegean.gr");
            };
        }
    }

}
