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

        private HashSet<string> LoadFavouriteKeys()
        {
          var json = Preferences.Get(FavouritePrefix, "[]");
            var keys = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            return keys.ToHashSet();
        }
        //Save the favourite movie keys to Preferences
        private void SaveFavouriteKeys(HashSet<string> keys)
        {
            var json = JsonSerializer.Serialize(keys.ToList());
            Preferences.Set(FavouritePrefix, json);
        }

        //Called to update the favourite status of a movie
        private void ApplyFavouriteStatus()
        {
            var favKeys = LoadFavouriteKeys();

            foreach (var movie in Movies)
            {
                 movie.IsFavourite = favKeys.Contains(movie.FavouriteKey);
            }
        }
        //Toggle the favourite status of a movie
        public void ToggleFavourite(Movie movie)
        {
            if (movie == null) return;
            var favKeys = LoadFavouriteKeys();
            if (movie.IsFavourite)
            {
                favKeys.Remove(movie.FavouriteKey);
                movie.IsFavourite = false;
            }
            else
            {
                favKeys.Add(movie.FavouriteKey);
                movie.IsFavourite = true;
            }
            SaveFavouriteKeys(favKeys);
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
                .SelectMany(m => m.Genre)
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
