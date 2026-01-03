using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieExplorer
{
    public class Movie
    {
        public string Title { get; set; }
        public int Year { get; set; }
        //JSON has an array for genre
        public List<string> Genre { get; set; } = new();
        public string Director { get; set; }
        public double Rating { get; set; }
        public string Emoji { get; set; }
        public bool IsFavourite { get; set; }
    }
}
