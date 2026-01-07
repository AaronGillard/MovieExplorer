using MovieExplorer.Services;

namespace MovieExplorer
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            ThemeService.ApplySavedTheme(this);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            

            //profile Picker verification
            return new Window(new NavigationPage(new ProfilePickerPage()));
        }
    }
}