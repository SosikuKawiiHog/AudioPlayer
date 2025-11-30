

namespace AudioPlayer
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            if(!File.Exists(Path.Combine(FileSystem.AppDataDirectory, "playlists.json")))
            {
                MainPage = new NavigationPage(new MainPage());
            }
            else
            {
                MainPage = new NavigationPage(new PlayerPage());
            }
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            // минимальный размер
            window.MinimumWidth = 1000;
            window.MinimumHeight = 600;

            return window;
        }
    }
}
