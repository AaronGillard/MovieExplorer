using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MovieExplorer.Services
{
    public static class JsonDownloadService
    {
        private const string JsonUrl = "https://raw.githubusercontent.com/DonH-ITS/jsonfiles/refs/heads/main/moviesemoji.json";

        private const string FileName = "moviesemoji.json";

        public static async Task<string> GetJsonFilePathAsync()
        {
            var filePath = Path.Combine(FileSystem.AppDataDirectory, FileName);

            if (File.Exists(filePath))
                return filePath;

            using var httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync(JsonUrl);
            await File.WriteAllTextAsync(filePath, json);

            return filePath;
        }
    }
}
