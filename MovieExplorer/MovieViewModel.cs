using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace MovieExplorer
{
    public class MovieViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public MovieViewModel()
        {
            Movies = new ObservableCollection<Movie>();
        }

        // This is what your CollectionView uses
        public ObservableCollection<Movie> Movies { get; }

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
            Movies.Clear();
            foreach (var movie in movies)
                Movies.Add(movie);
        }

        // Tells the UI “a property changed”
        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
