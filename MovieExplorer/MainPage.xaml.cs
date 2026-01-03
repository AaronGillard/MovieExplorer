using MovieExplorer.Services;

namespace MovieExplorer
{
    public partial class MainPage : ContentPage
    {
        // The ViewModel for data binding
        private readonly MovieViewModel _viewModel = new MovieViewModel();
        public MainPage()
        {
            InitializeComponent();
            // Set the BindingContext for data binding
            BindingContext = _viewModel;
        }

        //Temp Code
        private async void DownloadJson_Clicked(object sender, EventArgs e)
        {
            try
            {
                StatusLabel.Text = "Downloading...";

                var savedPath = await JsonDownloadService.DownloadJsonAsync();

                StatusLabel.Text = "Download complete. Loading movies...";

                await _viewModel.LoadMoviesAsync();

                StatusLabel.Text = $"Loaded {_viewModel.Movies.Count} movies.\nSaved to:\n{savedPath}";
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error:\n{ex.Message}";
            }
        }
        //End Temp Code
        private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
