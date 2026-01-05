using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Storage;
using System.Text.Json;
using System.Reflection.Metadata;

namespace MovieExplorer
{
    public class MovieViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public MovieViewModel()
        {
            Movies = new ObservableCollection<Movie>();
        }

        public ObservableCollection<string> Genres { get; } = new();

        //Filters collection by title
        private string _titleFilter = "";
        public string titleFilter
        {
            get => _titleFilter;
            set
            {
                if (_titleFilter == value) return;
                _titleFilter = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }

        //Filters collection by genre
        private string? _selectedGenre;
        public string? SelectedGenre
        {
            get => _selectedGenre;
            set
            {
                if (_selectedGenre == value) return;
                {
                    _selectedGenre = value;
                    OnPropertyChanged();
                    ApplyFilter();
                }
            }
        }

        // This is what your CollectionView uses
        public ObservableCollection<Movie> Movies { get; }
        //Stores ALL movies, without filtering
        private List<Movie> _allMovies = new();

        // This is what SelectedItem binds to
        private Movie? _selectedMovie;
        public Movie? SelectedMovie
        {
            get => _selectedMovie;
            set
            {
                if (_selectedMovie == value) return;
                _selectedMovie = value;
                OnPropertyChanged();
            }
        }
        
        private bool _showFavouritesOnly;
        public bool ShowFavouritesOnly
        {
            get => _showFavouritesOnly;
            set
            {
                if (_showFavouritesOnly == value) return;
                _showFavouritesOnly = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }

        // Load the JSON downloaded and saved earlier
        public async Task LoadMoviesAsync()
        {
            var filePath = Path.Combine(FileSystem.AppDataDirectory, "moviesemoji.json");

            // If the file isn't there, do nothing
            if (!File.Exists(filePath))
                return;

            // Read JSON text from file
            var jsonText = await File.ReadAllTextAsync(filePath);

            // Convert JSON into List<Movie>
            var movies = JsonSerializer.Deserialize<List<Movie>>(jsonText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<Movie>();

            // Put movies into the ObservableCollection (updates the UI)
            _allMovies = movies;

            BuildGenreList();

            //Apply favourite status from Preferences
            ApplyFavouriteStatus();
            //Updates UI with current filter
            ApplyFilter();
        }

        private const string FavouritePrefix = "favourite_movie_keys";


        //Called to update the favourite status of a movie
        private void ApplyFavouriteStatus()
        {
            var favourites = LoadFavouriteData();

            foreach (var movie in _allMovies)
            {
                if (favourites.TryGetValue(movie.FavouriteKey, out var favouritedDate))
                {
                    movie.IsFavourite = true;
                    movie.FavouritedOn = favouritedDate;
                }
                else
                {
                    movie.IsFavourite = false;
                    movie.FavouritedOn = null;
                }
            }
        }
        //Toggle the favourite status of a movie
        public void ToggleFavourite(Movie movie)
        {
            if(movie == null) return;

            var favourites = LoadFavouriteData();

            if (movie.IsFavourite)
            {
                //Unfavourite
                favourites.Remove(movie.FavouriteKey);
                movie.IsFavourite = false;
                movie.FavouritedOn = null;
            }
            else
            {
                //Favourite
                var now = DateTime.Now;
                favourites[movie.FavouriteKey] = now;
                movie.IsFavourite = true;
                movie.FavouritedOn = now;
            }

            SaveFavouriteData(favourites);
            //Re-apply filter to update UI
            ApplyFilter();
        }

        //Load favourite data with timestamps
        private Dictionary<string, DateTime> LoadFavouriteData()
        {
            var json = Preferences.Get(FavouritePrefix, "{}");

            // If old format exists (["key1","key2"]), migrate it
            if (!string.IsNullOrWhiteSpace(json) && json.TrimStart().StartsWith("["))
            {
                var oldKeys = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();

                var migrated = oldKeys
                    .Distinct()
                    .ToDictionary(k => k, _ => DateTime.Now);

                SaveFavouriteData(migrated);
                return migrated;
            }

            // New format ({"key":"2026-01-05T21:15:00"})
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, DateTime>>(json)
                       ?? new Dictionary<string, DateTime>();
            }
            catch
            {
                // If preferences got corrupted, don't crash the app
                return new Dictionary<string, DateTime>();
            }
        }

        //Save favourite data with timestamps
        private void SaveFavouriteData(Dictionary<string, DateTime> data)
        {
            var json = JsonSerializer.Serialize(data);
            Preferences.Set(FavouritePrefix, json);
        }
        private void ApplyFilter()
        {
            IEnumerable<Movie> query = _allMovies;

            // 1) Favourites-only filter
            if (ShowFavouritesOnly)
                query = query.Where(m => m.IsFavourite);

            // 2) Title filter (partial match)
            if (!string.IsNullOrWhiteSpace(titleFilter))
            {
                var text = titleFilter.Trim();
                query = query.Where(m => !string.IsNullOrWhiteSpace(m.Title) &&
                                         m.Title.Contains(text, StringComparison.OrdinalIgnoreCase));
            }

            // 3) Genre filter
            if (!string.IsNullOrWhiteSpace(SelectedGenre) && SelectedGenre != "All")
            {
                var g = SelectedGenre;
                query = query.Where(m => m.Genre != null && m.Genre.Contains(g));
            }

            // Update the UI list
            Movies.Clear();
            foreach (var movie in query)
                Movies.Add(movie);
        }
        //Helper method to build the genre list for filtering
        private void BuildGenreList()
        {
            Genres.Clear();
            Genres.Add("All");

            var allGenres = _allMovies
                .SelectMany(m => m.Genre ?? new List<string>())
                .Distinct()
                .OrderBy(g => g);

            foreach(var g in allGenres)
                Genres.Add(g);

            //Default Selection
            if(SelectedGenre == null)
                SelectedGenre = "All";
        }

        // Tells the UI “a property changed”
        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
