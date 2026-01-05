using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MovieExplorer
{
    public class Movie : INotifyPropertyChanged
    {
        //When triggered, notifies the UI that a property value has changed
        public event PropertyChangedEventHandler? PropertyChanged;
        public string Title { get; set; }
        public int Year { get; set; }
        //JSON has an array for genre
        public List<string> Genre { get; set; } = new();
        public string Director { get; set; }
        public double Rating { get; set; }
        public string Emoji { get; set; }
        


        // Computed property to get genre as a comma-separated string
        // Implemented as <Label - GenreText can only display text, not a list>
        public string GenreText =>
           Genre != null && Genre.Count > 0
           ? string.Join(", ", Genre)
           : string.Empty;

        //backing field for IsFavourite property

        private bool _IsFavourite;
        public bool IsFavourite
        {
            get => _IsFavourite;
            set
            {
                if (_IsFavourite != value)
                {
                    _IsFavourite = value;
                    OnPropertyChanged();
                }
            }
        }
        //Creates Unique ID for each movie to be stored in Preferences
        public string FavouriteKey => $"{Title}|{Year}";
        //helper method for raising the PropertyChanged event
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
      
    }
}
