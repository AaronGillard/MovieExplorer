using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace MovieExplorer.Services
{
    public static class JsonDownloadService
    {
        private const string JsonUrl = "https://raw.githubusercontent.com/DonH-ITS/jsonfiles/refs/heads/main/moviesemoji.json";

        private const string FileName = "moviesemoji.json";

        public static async Task<string> DownloadJsonAsync()
        {
            // Where the file will be saved
            var filePath = Path.Combine(FileSystem.AppDataDirectory, FileName);

            //Reuse existing file if it exists
            if (File.Exists(filePath))
            {
                return filePath;
            }
            // Download the JSON
            using var httpClient = new HttpClient();
            var jsonContent = await httpClient.GetStringAsync(JsonUrl);

            // Save locally
            await File.WriteAllTextAsync(filePath, jsonContent);

            return filePath; //Returns path so it can be shown for testing purposes
        }
    }
}
