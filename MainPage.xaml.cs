using Microsoft.Maui.Controls;

namespace AudioPlayer;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnOpenFolderClicked(object sender, EventArgs e)
    {
        // тут потом открыть папку типо
        //await Navigation.PushAsync(new PlayerPage(), animated: false);
        Application.Current.MainPage = new PlayerPage();

    }
}
