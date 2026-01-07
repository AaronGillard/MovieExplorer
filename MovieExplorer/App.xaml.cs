namespace MovieExplorer
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            //Temp disable app shell for profile picker testing
            //return new Window(new AppShell());

            //profile Picker verification
            return new Window(new NavigationPage(new ProfilePickerPage()));
        }
    }
}