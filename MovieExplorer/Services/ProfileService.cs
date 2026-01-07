using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MovieExplorer.Models;

namespace MovieExplorer.Services
{
    class ProfileService
    {
        private const string CurrentProfileIdKey = "current_profile_id";
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        private string ProfilesPath => Path.Combine(FileSystem.AppDataDirectory, "profiles.json");
        public string? GetCurrentProfileId()
        {
            var id = Preferences.Get(CurrentProfileIdKey, "");
            return string.IsNullOrEmpty(id) ? null : id;
        }
        public void SetCurrentProfileId(string profileId)
        {
            Preferences.Set(CurrentProfileIdKey, profileId);
        }

        public async Task<List<Profile>> LoadProfilesAsync()
        {
            if (!File.Exists(ProfilesPath))
                return new List<Profile>();
            
            var json = await File.ReadAllTextAsync(ProfilesPath);
           return JsonSerializer.Deserialize<List<Profile>>(json, JsonOptions) ?? new List<Profile>();
        }

        //Ensures the profiles file exists and  ensures CurrentProfileId is set
        public async Task SaveProfilesAsync(List<Profile> profiles)
        {
            var json = JsonSerializer.Serialize(profiles, JsonOptions);
            await File.WriteAllTextAsync(ProfilesPath, json);
        }
        public async Task <Profile> EnsureCurrentProfileAsync()
        {
            var profiles = await LoadProfilesAsync();

            if(profiles.Count == 0)
            {
                profiles.Add(new Profile { Name = "Default" });
                await SaveProfilesAsync(profiles);
            }

            var currentId = GetCurrentProfileId();
            var current = profiles.FirstOrDefault(p => p.Id == currentId) ?? profiles[0];

            if (currentId != current.Id)
            {
                SetCurrentProfileId(current.Id);
            }

            return current;
        }
    }
}
