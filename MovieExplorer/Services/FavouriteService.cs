using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MovieExplorer.Services
{
    class FavouriteService
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        private static string GetPath(string profileId) => Path.Combine(FileSystem.AppDataDirectory, $"favourites_{profileId}.json");

        // Returns FavouriteKey -> favourited time in UTC
        public async Task<Dictionary<string, DateTime>> LoadAsync(string profileId)
        {
            var path = GetPath(profileId);
            if (!File.Exists(path))
                return new Dictionary<string, DateTime>();

            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<Dictionary<string, DateTime>>(json, JsonOptions) ?? new Dictionary<string, DateTime>();
        }

        public async Task SaveAsync(string profileId, Dictionary<string, DateTime> favourites)
        {
            var path = GetPath(profileId);
            var json = JsonSerializer.Serialize(favourites, JsonOptions);
            await File.WriteAllTextAsync(path, json);
        }
    }
}
