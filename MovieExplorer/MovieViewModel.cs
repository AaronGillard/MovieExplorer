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
using MovieExplorer.Services;

namespace MovieExplorer
{
    public class MovieViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly ProfileService _profileService = new();
        private readonly FavouriteService _favouriteService = new();

        private string? _currentProfileId;
        private Dictionary<string, DateTime> _favouritesByKey = new();

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

            await EnsureFavouritesLoadedAsync(forceReload: true);
            ApplyFilter();
        }

        
        //Toggle the favourite status of a movie
        public async Task ToggleFavourite(Movie movie)
        {
            if (movie == null) return;

            // Always ensure the favourites dictionary matches the currently selected profile
            await EnsureFavouritesLoadedAsync();

            var key = movie.FavouriteKey;

            if (movie.IsFavourite)
            {
                _favouritesByKey.Remove(key);
                movie.IsFavourite = false;
                movie.FavouritedOn = null;
            }
            else
            {
                var nowUtc = DateTime.UtcNow;
                _favouritesByKey[key] = nowUtc;

                movie.IsFavourite = true;
                movie.FavouritedOn = nowUtc.ToLocalTime();
            }

            await _favouriteService.SaveAsync(_currentProfileId!, _favouritesByKey);
            ApplyFilter();
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
        //Ensure favourites are loaded for the current profile
        private async Task EnsureFavouritesLoadedAsync(bool forceReload = false)
        {
            var profile = await _profileService.EnsureCurrentProfileAsync();
            var activeProfileId = profile.Id;

            // Reload if forced OR if the profile changed
            if (forceReload || !string.Equals(_currentProfileId, activeProfileId, StringComparison.Ordinal))
            {
                _currentProfileId = activeProfileId;
                _favouritesByKey = await _favouriteService.LoadAsync(_currentProfileId);

                // Apply to movies, including clearing IsFavourite/FavouritedOn when not favourited
                ApplyFavouriteStatusFromLoadedFavourites();
            }
        }

        private void ApplyFavouriteStatusFromLoadedFavourites()
        {
            foreach (var movie in _allMovies)
            {
                if (_favouritesByKey.TryGetValue(movie.FavouriteKey, out var whenUtc))
                {
                    movie.IsFavourite = true;
                    movie.FavouritedOn = whenUtc.ToLocalTime();
                }
                else
                {
                    //Stops favourites from persisting after profile change
                    movie.IsFavourite = false;
                    movie.FavouritedOn = null;
                }
            }
        }

        //Wrapper to allow forcing reload of favourites
        public Task ToggleFavouriteStatusAsync(Movie movie) => ToggleFavourite(movie);
       
        // Tells the UI “a property changed”
        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
