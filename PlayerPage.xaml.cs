using Microsoft.Maui.Controls;

namespace AudioPlayer;

public partial class PlayerPage : ContentPage
{
    private bool isFullScreen = false;

    public PlayerPage()
    {
        InitializeComponent();
    }

    private void OnFullScreenClicked(object sender, EventArgs e)
    {
        isFullScreen = !isFullScreen;
        NormalLayout.IsVisible = !isFullScreen;
        FullScreenLayout.IsVisible = isFullScreen;
    }
}
