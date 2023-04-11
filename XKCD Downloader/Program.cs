using System;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace XKCD_Downloader
{
    class Program
    {
        static void Main(string[] args)
        {
            var doc = new HtmlDocument();
            var web = new HtmlWeb();

            string link = "https://xkcd.com";

            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string dirName = Path.Combine(userPath, @"Pictures\XKCD Comics");
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            //Loops until it reaches the first comic
            do
            {
                while (true)
                {
                    try
                    {
                        //Loads each comic's page and stores the HTML for parsing
                        doc = web.Load(link);
                        break;
                    }
                    catch (WebException)
                    {
                        Thread.Sleep(3000);
                        continue;
                    }
                }
                
                try
                {
                    //Gets comic's details
                    string comicName = doc.DocumentNode.SelectSingleNode("//div[@id='comic']//img")
                        .GetAttributeValue("alt", null);
                    string comicNumber = doc.DocumentNode.SelectSingleNode("//div[@id='middleContainer']/a[1]")
                        .InnerText.Split('/')[3];
                    string comicImage = doc.DocumentNode.SelectSingleNode("//div[@id='comic']//img")
                        .GetAttributeValue("src", null);

                    //Downloads comic
                    DownloadComic(comicImage, comicNumber, comicName, dirName);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine("Interactive comic detected. Skipping...");
                }
                
                //Gets link to previous comic
                string hrefVal = doc.DocumentNode.SelectSingleNode("//*[@id='middleContainer']/ul[1]/li[2]/a")
                    .GetAttributeValue("href", null);
                link = $"https://xkcd.com{hrefVal}";
                Thread.Sleep(2000);

            } while (!link.EndsWith("#"));
        }

        static void DownloadComic(string img, string number, string name, string dir)
        {
            //Parses names correctly
            name = checkName(name);
            string file = Path.Combine(dir, $"Comic #{number}, {name}.png");

            //Deletes file if it's empty (has no image stored)
            FileInfo info = new FileInfo(file);
            if (File.Exists(file) && info.Length == 0)
                File.Delete(file);

            //Skips file if it has already been downloaded.
            else if (File.Exists(file))
            {
                Console.WriteLine($"Comic #{number} already downloaded. Skipping...");
                return;
            }

            Console.WriteLine($"Downloading Comic #{number}...");

            //Requests comic image and saves it to the system
            using (WebClient client = new WebClient())
            {
                client.DownloadFile($"https:{img}", file);
            }

            Console.WriteLine($"Successfully downloaded to {dir}.");
        }

        static string checkName (string name)
        {
            char[] illegalChars = Path.GetInvalidFileNameChars();

            foreach (char character in illegalChars)
            {
                if (name.Contains(character.ToString()))
                    name = name.Replace(character.ToString(), "");
            }

            name = name.Replace("&#39;", "'");

            return name;
        }
    }
}