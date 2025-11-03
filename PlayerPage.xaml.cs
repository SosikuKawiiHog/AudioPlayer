using Microsoft.Maui.Controls;

namespace AudioPlayer;

public partial class PlayerPage : ContentPage
{
    private bool isFullScreen = false;
    private bool _isExpanded = true;

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

    private void OnAddMoreTracks(object sender, EventArgs e)
    {
        // здесь снова должен открываться проводник но с настройкой на один или несколько mp3-wav-другие аудио
        // обработка там типо в temp кинуть ну тут сами думайте
    }

    // для сворачивания плейлиста
    private void OnToggleClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.BindingContext is VerticalStackLayout tracksContainer)
        {
            // Переключаем видимость
            tracksContainer.IsVisible = !tracksContainer.IsVisible;

            // Меняем иконку
            button.Source = tracksContainer.IsVisible
                ? "collapse_icon.png"
                : "expand_icon.png";
        }
    }

    private int _playlistCount = 1;

    private void OnCreatePlaylistClicked(object sender, EventArgs e)
    {
        // должен создаться плейлист - json файл структуры которой нету
        // в структуре того что должно отобразиться указываеться количество песен
        // ну короче да сделаете

        // короче здесь просто создаётся базированный не читаемый не с json но в будущем парситься с json


        // контейнер для одного плейлиста
        var playlistLayout = new VerticalStackLayout();
        var headerGrid = new Grid
        {
            ColumnDefinitions =
        {
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            new ColumnDefinition { Width = GridLength.Auto }
        }
        };

        // контейнер треков
        var tracksContainer = new VerticalStackLayout
        {
            Margin = new Thickness(10, 10, 0, 0),
            Spacing = 8,
            BackgroundColor = Color.FromArgb("#2C2D2B"),
            IsVisible = true // стартовое состояние — развёрнут
        };
        tracksContainer.Add(CreateTrackFrame("C418 - Far"));
        tracksContainer.Add(CreateTrackFrame("music - cool"));

        // кнопка сворачивания
        var collapseButtonContainer = new Grid
        {
            WidthRequest = 20,
            HeightRequest = 20,
            Margin = new Thickness(15, 0)
        };

        var toggleButton = new ImageButton
        {
            Source = "collapse_icon.png",
            BackgroundColor = Colors.Transparent,
            Aspect = Aspect.AspectFit,
            BindingContext = tracksContainer // какой блок сворачивать
        };
        toggleButton.Clicked += OnToggleClicked;
        collapseButtonContainer.Add(toggleButton);
        headerGrid.Add(collapseButtonContainer, 0, 0);

        var playButton = new Button
        {
            ImageSource = "button_play.png",
            WidthRequest = 50,
            BackgroundColor = Colors.Transparent
        };
        headerGrid.Add(playButton, 1, 0);

        var playlistLabel = new Label
        {
            Text = $"Плейлист {_playlistCount++}",
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            FontSize = 18,
            VerticalOptions = LayoutOptions.Center
        };
        headerGrid.Add(playlistLabel, 2, 0);

        var tracksCountLabel = new Label
        {
            Text = "5 треков",
            TextColor = Color.FromArgb("#A8A8A8"),
            VerticalOptions = LayoutOptions.Center
        };
        headerGrid.Add(tracksCountLabel, 3, 0);

        playlistLayout.Add(headerGrid);
        playlistLayout.Add(tracksContainer);

        PlaylistsContainer.Add(playlistLayout);
    }

    // для создания одного трека
    private Frame CreateTrackFrame(string trackName)
    {
        return new Frame
        {
            BackgroundColor = Color.FromArgb("2C2D2B"),
            BorderColor = Color.FromArgb("Transparent"),
            HasShadow = false,
            Padding = 0,
            CornerRadius = 8,
            Content = new HorizontalStackLayout
            {
                Children =
            {
                new Button
                {
                    ImageSource = "button_play.png",
                    WidthRequest = 50,
                    BackgroundColor = Colors.Transparent,
                    VerticalOptions = LayoutOptions.Center
                },
                new Label
                {
                    Text = trackName,
                    TextColor = Colors.White,
                    FontSize = 16,
                    VerticalOptions = LayoutOptions.Center
                }
            }
            }
        };
    }
}
