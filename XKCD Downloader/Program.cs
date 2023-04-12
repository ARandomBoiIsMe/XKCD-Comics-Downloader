using System;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace XKCD_Downloader
{
    class Program
    {
        static async Task Main()
        {
            var client = new HttpClient();

            string comicURL = "https://xkcd.com/info.0.json";
            var comic = await getComicDetails(client, comicURL);

            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string dirName = Path.Combine(userPath, @"Pictures\XKCD Comics");
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            //Moves through comics from the most recent to the first
            do
            {
                
                await DownloadComic(client, comic.img, comic.num, comic.safe_title, dirName);

                comicURL = $"https://xkcd.com/{ int.Parse(comic.num) - 1 }/info.0.json";
                comic = await getComicDetails(client, comicURL);

            } while (comic != null);

            Console.WriteLine("All XKCD comics have been downloaded (Probably)." +
                            "\nPress any key to exit the program.");
            Console.ReadKey();
        }

        static async Task<Comic> getComicDetails(HttpClient client, string comicURL)
        {
            var response = await client.GetAsync(comicURL);
            var responseBody = await response.Content.ReadAsStringAsync();

            try
            {
                var comic = JsonConvert.DeserializeObject<Comic>(responseBody);
                return comic;
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }

        static async Task DownloadComic(HttpClient client, string img, string number, string name, string dir)
        {
            name = checkName(name);
            string file = Path.Combine(dir, $"Comic #{number}, {name}.png");

            //Deletes file if it's empty (has no image stored)
            FileInfo info = new FileInfo(file);
            if (File.Exists(file) && info.Length == 0)
                File.Delete(file);

            else if (File.Exists(file))
            {
                Console.WriteLine($"Comic #{number} already downloaded. Skipping...");
                return;
            }

            Console.WriteLine($"Downloading Comic #{number}...");

            //Requests comic image and saves it to the system
            var response = await client.GetAsync(img);
            var imageStream = await response.Content.ReadAsStreamAsync();

            using (FileStream fileStream = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                await imageStream.CopyToAsync(fileStream);
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