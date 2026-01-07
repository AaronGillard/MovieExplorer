using MovieExplorer.Services;

namespace MovieExplorer
{
    //PRE PROFILES BUILD FOR SECURITY
    public partial class MainPage : ContentPage
    {
        // The ViewModel for data binding
        private readonly MovieViewModel _viewModel = new MovieViewModel();
        private bool _loadedOnce;
        public MainPage()
        {
            InitializeComponent();
            // Set the BindingContext for data binding
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {

            base.OnAppearing();

            if (_loadedOnce) return;
            _loadedOnce = true;

            try
            {
                // Ensures JSON exists locally (downloads only on first run)
                await JsonDownloadService.GetJsonFilePathAsync();

                // Loads movies from the local file into the ObservableCollection
                await _viewModel.LoadMoviesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Startup load failed: {ex}");
                await DisplayAlert("Loading failed", ex.Message, "OK");
            }
        }

        private async void Favourite_Clicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is Movie movie)
                await _viewModel.ToggleFavouriteStatusAsync(movie);
        }

        private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
