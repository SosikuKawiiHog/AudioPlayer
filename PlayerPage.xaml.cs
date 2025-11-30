using AudioPlayer.Models;
using AudioPlayer.Services;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using TagLib;
using Windows.Devices.Radios;
using static System.Net.Mime.MediaTypeNames;
using Application = Microsoft.Maui.Controls.Application;
using File = System.IO.File;
namespace AudioPlayer;

public partial class PlayerPage : ContentPage
{

    //private bool isFullScreen = false;
    private bool isUserSeeking = false;
    private bool isRepeatEnabled = false;
    private bool isShuffleEnabled = false;
    private Random random = new();
    //private bool _isExpanded = true;
    //private int _playlistCount = 1;

    public PlayerPage()
    {
        InitializeComponent();
        BindingContext = AudioManager.Instance;

        //связь  со слайдером музыки, багнутая чутка
        positionSlider.DragCompleted += (s, e) =>
        {
            if (mediaElement.Duration > TimeSpan.Zero && isUserSeeking)
            {
                var newPos = TimeSpan.FromSeconds(positionSlider.Value);
                mediaElement.SeekTo(newPos);
            }
            isUserSeeking = false;
        };
        positionSlider.DragStarted += (s, e) =>
        {
            isUserSeeking = true;
        };


        mediaElement.MediaEnded += OnMediaEnded;
        AudioManager.Instance.Playlists.CollectionChanged += OnTrackFrameLoaded;
        AudioManager.Instance.Playlists.CollectionChanged += OnPlaylistFrameLoaded;
    }


    //включить плейлист
    private void OnPlaylistPlayClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Playlist playlist)
        {
            if(playlist.Tracks == null) //вроде бы уже не нужна проверка
            {
                DisplayAlert("Ошибка!", "Плейлист пуст!", "блин");
                return;
            }

            if (playlist.Tracks.Count == 0)
            {
                DisplayAlert("Ошибка!", "Плейлист пуст!", "блин");
                return;
            }

            var queue = AudioManager.Instance.Queue;
            queue.Clear();
            foreach(var track in playlist.Tracks)
            {
                queue.Add(track);
            }

            if(queue.Count > 0)
            {
                var first = queue[0];
                PlayTrack(first);
            }
        }
    }
    private async void OnAddMoreTracks(object sender, EventArgs e)
    {
        // здесь снова должен открываться папка, но с настройкой на один или несколько mp3/wav/другие х
        // потому ну обработка сами короче я хззз просто куда в temp типо

        //услышал тебя родной

        try
        {
            var folderPicker = FolderPicker.Default;
            var result = await folderPicker.PickAsync(CancellationToken.None);

            if (result.IsSuccessful && result.Folder != null)
            {
                var allowedExtensions = new[] { ".mp3", ".wav", ".ogg", ".flac" };

                var files = Directory.GetFiles(result.Folder.Path).Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant())).ToArray();

                if (files.Length == 0)
                {
                    await DisplayAlert("Ошибка!", "В выбранной папке нет аудиофайлов.", "блин");
                    return;
                }

                var temp = AudioManager.Instance.TempPlaylist;
                if (temp == null)
                {
                    AudioManager.Instance.LoadTracksFromPaths(files);
                }
                else
                {
                    foreach (var file in files)
                    {
                        var filename = Path.GetFileNameWithoutExtension(file);
                        byte[]? coverDataTemp = null;
                        string artist = "Unknown";
                        string name = filename;
                        try
                        {
                            using (var tagFile = TagLib.File.Create(file))
                            {
                                var pictures = tagFile.Tag.Pictures;
                                if (pictures.Length > 0)
                                {
                                    coverDataTemp = pictures[0].Data.Data;
                                }
                                var performers = tagFile.Tag.Performers;
                                if (performers.Length > 0)
                                {
                                    artist = performers[0];
                                }
                                if (tagFile.Tag.Title != null)
                                {
                                    name = tagFile.Tag.Title;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            coverDataTemp = null;
                        }

                        temp.Tracks.Add(new Track
                        {
                            Path = file,
                            Title = name,
                            Artist = artist,
                            CoverData = coverDataTemp
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка!", ex.Message, "блин");
        }
    }

    //создание плейлиста, для контекстной хрени также обновляется весь уи. ИНАЧЕ Я ХЗ ЧЁ ДЕЛАТЬ
    private void OnCreatePlaylistClicked(object sender, EventArgs e)
    {
        var newPlaylist = new Playlist
        {
            Name = $"Плейлист {AudioManager.Instance.Playlists.Count + 1}",
            IsExpanded = true,
            Tracks = new System.Collections.ObjectModel.ObservableCollection<Track>()
        };

        AudioManager.Instance.Playlists.Add(newPlaylist);
        var bc = BindingContext;
        BindingContext = null;
        BindingContext = bc;
    }

    //свертка и развертка плейлиста
    private void OnTogglePlaylistClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Playlist playlist)
        {
            playlist.IsExpanded = !playlist.IsExpanded;
        
        // меняем иконку
        btn.ImageSource = playlist.IsExpanded
            ? "collapse_icon.png"
            : "expand_icon.png";
        }
    }

    //выбрали произвольный трек из плейлиста
    private void OnTrackPlayClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Track track)
        {
            

            var manager = AudioManager.Instance;
            var parent = btn.Parent;

            Playlist? parentPlaylist = null;

            while(parent != null)
            {
                if(parent is CollectionView collectionView)
                {
                    if(collectionView.ItemsSource is System.Collections.IEnumerable tracks)
                    {
                        foreach(var playlist in AudioManager.Instance.Playlists)
                        {
                            if(playlist.Tracks == tracks)
                            {
                                parentPlaylist = playlist;
                            }
                        }
                    }
                }
                parent = parent.Parent;
            }


            if (parentPlaylist != null)
            {
                var index = parentPlaylist.Tracks.IndexOf(track);
                var tracksToPlay = parentPlaylist.Tracks.Skip(index).ToList();

                manager.Queue.Clear();
                foreach(var t in tracksToPlay)
                {
                    manager.Queue.Add(t);
                }
            }

            PlayTrack(track);
        }
    }
    //выбрали произвольный трек из очереди
    private void OnQueueTrackPlayClicked(object sender, EventArgs e)
    {
        if(sender is Button btn && btn.CommandParameter is Track track)
        {
            PlayTrack(track);
        }
    }

    private async void PlayTrack(Track track)
    {
        //System.Diagnostics.Debug.WriteLine($" SAERMOU{mediaElement.CurrentState}");
        //if (mediaElement.CurrentState != MediaElementState.Stopped)
        //{
        //    mediaElement.Stop();
        //    mediaElement.Source = null;
        //    await Task.Delay(50);
        //}
        //System.Diagnostics.Debug.WriteLine($"saermotest2 {mediaElement.Source}");
        
        mediaElement.Stop();
        mediaElement.Source = null;
        AudioManager.Instance.CurrentTrack = track;
        AudioManager.Instance.IsPlaying = true;
        mediaElement.Source = MediaSource.FromFile(track.Path);
        await Task.Delay(100);
        mediaElement.Play();
        PlayerBarPlayButton.ImageSource = "button_play_pause.png";
     
    }

    //если трек кончился
    private async void OnMediaEnded(object sender, EventArgs e)
    {
        if(isRepeatEnabled && AudioManager.Instance.CurrentTrack != null)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            { PlayTrack(AudioManager.Instance.CurrentTrack!); });
            return;
        }
        var queue = AudioManager.Instance.Queue;
        var current = AudioManager.Instance.CurrentTrack;

        if(current != null && queue.Contains(current))
        {
            var index = queue.IndexOf(current);
            Track? next = null;

            if(index + 1 < queue.Count)
            {
                next = queue[index + 1];
            }
            else
            {
                next = queue.Count > 0 ? queue[0] : null;
            }
            if(next != null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                { PlayTrack(next!); });
            }
        }
    }

    //контексное меню для добавления в плейлисты
    private void OnTrackFrameLoaded(object sender, EventArgs e)
    {
        if(sender is Frame frame && frame.BindingContext is Track track)
        {
            var menu = new MenuFlyout();

            var addToPlaylist = new MenuFlyoutSubItem { Text = "Добавить в плейлист" };
            var permanentPlaylists = AudioManager.Instance.Playlists.Where(p => !p.IsTemporary).ToList();
            FlyoutBase.SetContextFlyout(frame, null);
            if (permanentPlaylists.Count == 0)
            {
                addToPlaylist.Add(new MenuFlyoutItem { Text = "Нет плейлистов", IsEnabled = false });
            }
            else
            {
                foreach (var playlist in permanentPlaylists)
                {
                    var item = new MenuFlyoutItem { Text = playlist.Name };
                    item.Clicked += (s, e) =>
                    {
                        playlist.Tracks ??= new System.Collections.ObjectModel.ObservableCollection<Track>();
                        if (!playlist.Tracks.Contains(track))
                        {
                            playlist.Tracks.Add(track);
                        }
                    };
                    addToPlaylist.Add(item);
                }
            }
            menu.Add(addToPlaylist);

            var parentPlaylist = AudioManager.Instance.Playlists
                .FirstOrDefault(p => p.Tracks.Contains(track) && !p.IsTemporary);
            if (parentPlaylist != null)
            {
                var deleteItem = new MenuFlyoutItem { Text = "Удалить из плейлиста" };
                deleteItem.Clicked += (s, e) =>
                {
                    parentPlaylist.Tracks.Remove(track);
                };
                menu.Add(deleteItem);
            }

            FlyoutBase.SetContextFlyout(frame,menu);
        }
    }
    // Контекстное меню для изменения названия плейлиста
    private void OnPlaylistFrameLoaded(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is Playlist playlist && !playlist.IsTemporary)
        {
            var menu = new MenuFlyout();

            var changeName = new MenuFlyoutItem { Text = "Изменить название" };
            changeName.Clicked += async (s, args) => await OnChangeNameClicked(playlist);

            menu.Add(changeName);

            FlyoutBase.SetContextFlyout(frame, menu);
        }
    }

    private async Task OnChangeNameClicked(Playlist playlist)
    {
        // Запрос нового названия
        string newName = await Application.Current.MainPage.DisplayPromptAsync(
            "Изменение названия",
            "Введите новое название плейлиста:",
            initialValue: playlist.Name,
            maxLength: 20,
            keyboard: Keyboard.Text);

        // Проверяем, что пользователь не нажал "Отмена" и ввел не пустое значение
        if (!string.IsNullOrWhiteSpace(newName))
        {
            playlist.Name = newName;
            var bc = BindingContext;
            BindingContext = null;
            BindingContext = bc;
        }
    }

    private void OnRepeatClicked(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            isRepeatEnabled = !isRepeatEnabled;
            btn.ImageSource = isRepeatEnabled ? "repeat_on.png" : "repeat.png";
        }
    }


    private void OnShuffleClicked(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            isShuffleEnabled = !isShuffleEnabled;
            btn.ImageSource = isShuffleEnabled ? "shuffle_on.png": "shuffle.png";
            if (isShuffleEnabled && AudioManager.Instance.Queue.Count > 1)
            {
                var queue = AudioManager.Instance.Queue.ToList();
                var current = AudioManager.Instance.CurrentTrack;

                queue.RemoveAll(t => t.Path == current?.Path);

                //ya skuchayu po plusam... :(
                var shuffled = queue.OrderBy(x => random.Next()).ToList();

                if (current != null) shuffled.Insert(0, current);

                AudioManager.Instance.Queue.Clear();
                foreach (var track in shuffled) AudioManager.Instance.Queue.Add(track);
            }
        }
    }

    private void OnPrevTrackClicked(object sender, EventArgs e)
    {
        var queue = AudioManager.Instance.Queue;
        var current = AudioManager.Instance.CurrentTrack;

        if(current == null || queue.Count == 0) return;

        int currentIndex = queue.IndexOf(current);
        if(currentIndex < 0) return;
        Track? prev = null;

        if(currentIndex > 0)
        {
            prev = queue[currentIndex - 1];
        }
        else
        {
            prev = queue.Count > 0 ? queue[^1] : null;
        }
        if (prev != null)
        {
            PlayTrack(prev);
        }
    }

    private void OnNextTrackClicked(object sender, EventArgs e)
    {
        var queue = AudioManager.Instance.Queue;
        var current = AudioManager.Instance.CurrentTrack;

        if (current == null || queue.Count == 0) return;

        int currentIndex = queue.IndexOf(current);
        if (currentIndex < 0) return;
        Track? next = null;

        if (currentIndex + 1 < queue.Count)
        {
            next = queue[currentIndex + 1];
        }
        else
        {
            next = queue.Count > 0 ? queue[0] : null;
        }
        if (next != null)
        {
            PlayTrack(next);
        }
    }

    private void OnGlobalPlayPauseClicked(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            if (AudioManager.Instance.IsPlaying)
            {
                mediaElement.Pause();
                AudioManager.Instance.IsPlaying = false;
                PlayerBarPlayButton.ImageSource = "button_play.png";
            }
            else if (AudioManager.Instance.CurrentTrack != null)
            {
                mediaElement.Play();
                AudioManager.Instance.IsPlaying = true;
                PlayerBarPlayButton.ImageSource = "button_play_pause.png";
            }
        }
    }


}
