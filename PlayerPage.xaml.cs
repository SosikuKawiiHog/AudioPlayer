using Microsoft.Maui.Controls;

namespace AudioPlayer;

public partial class PlayerPage : ContentPage
{
    private bool isFullScreen = false;
    private bool _isExpanded = true;
    private int _playlistCount = 1;

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
        // здесь снова должен открываться папка, но с настройкой на один или несколько mp3/wav/другие х
        // потому ну обработка сами короче я хззз просто куда в temp типо
    }


    // для сворачивания плейлиста
    private void OnToggleClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.BindingContext is VerticalStackLayout tracksContainer)
        {
            tracksContainer.IsVisible = !tracksContainer.IsVisible;

            button.Source = tracksContainer.IsVisible
                ? "collapse_icon.png"
                : "expand_icon.png";
        }
    }

    private void OnCreatePlaylistClicked(object sender, EventArgs e)
    {
        // должен создаться плейлист - json файл структуры которой нету
        // в структуре того что должно отобразиться указываеться количество песен
        // ну короче да сделаете

        // короче здесь просто создаётся базированный не читаемый не с json но в будущем парситься с json


        // создаём контейнер для одного плейлиста
        var playlistLayout = new VerticalStackLayout();

        var headerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Margin = new Thickness(0, 0, 0, 0), // ⬅️ Сдвигаем весь блок чуть влево
            ColumnSpacing = 0
        };


        var tracksContainer = new VerticalStackLayout
        {
            Margin = new Thickness(10, 10, 0, 0),
            Spacing = 5,
            BackgroundColor = Color.FromArgb("#2F2F2F"),
            IsVisible = true
        };

        tracksContainer.Add(CreateTrackFrame("C418 - Far"));
        tracksContainer.Add(CreateTrackFrame("music - cool"));

        var collapseButtonContainer = new Grid
        {
            WidthRequest = 30,
            HeightRequest = 30,
            Margin = new Thickness(0, 0)
        };

        var toggleButton = new ImageButton
        {
            Source = "collapse_icon.png",
            WidthRequest = 20,
            HeightRequest = 20,
            BackgroundColor = Colors.Transparent,
            Aspect = Aspect.AspectFit,
            VerticalOptions = LayoutOptions.Center,
            BindingContext = tracksContainer, // какой блок сворачивать
            Margin = new Thickness(0, -7)
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
            VerticalOptions = LayoutOptions.Center,
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
        var trackLayout = new HorizontalStackLayout
        {
            Spacing = 0,
            VerticalOptions = LayoutOptions.Center,
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
        };

        // контекстное меню

        // короче получается так,что сейчас контекстое меню формируется один раз и когда
        // мы добавим следущие плейлисты они отображаться не будут 
        // ну логично 0_0
        

        var menu = new MenuFlyout();

        var addToPlaylist = new MenuFlyoutSubItem { Text = "Добавить в плейлист" };

        if (PlaylistsContainer != null && PlaylistsContainer.Children.Count > 0)
        {
            foreach (var playlist in PlaylistsContainer.Children.OfType<VerticalStackLayout>())
            {
                var label = playlist.Children
                    .OfType<Grid>()
                    .FirstOrDefault()?
                    .Children
                    .OfType<Label>()
                    .FirstOrDefault();

                var name = label?.Text ?? "Без названия";

                var item = new MenuFlyoutItem { Text = name };
                item.Clicked += (s, e) =>
                {
                    Application.Current?.MainPage?.DisplayAlert("я", "все понял", "спончбоб");
                };
                addToPlaylist.Add(item);
            }
        }
        else
        {
            addToPlaylist.Add(new MenuFlyoutItem { Text = "нет плейлистов" });
        }

        menu.Add(addToPlaylist);

        var deleteItem = new MenuFlyoutItem { Text = "Удалить из плейлиста" };
        deleteItem.Clicked += (s, e) =>
        {
            if (trackLayout.Parent is Frame frame && frame.Parent is Layout parent)
                parent.Children.Remove(frame);
        };
        menu.Add(deleteItem);

        FlyoutBase.SetContextFlyout(trackLayout, menu);

        return new Frame
        {
            BackgroundColor = Color.FromArgb("#2F2F2F"),
            Padding = 0,
            CornerRadius = 10,
            Content = trackLayout
        };
    }
}