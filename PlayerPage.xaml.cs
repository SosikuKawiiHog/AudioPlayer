using AudioPlayer.Models;
using AudioPlayer.Services;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AudioPlayer;

public partial class PlayerPage : ContentPage
{

    private bool isFullScreen = false;
    private bool isUserSeeking = false;
    //private bool _isExpanded = true;
    //private int _playlistCount = 1;

    public PlayerPage()
    {
        InitializeComponent();
        BindingContext = AudioManager.Instance;
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
    }

    private void OnFullScreenClicked(object sender, EventArgs e)
    {
        isFullScreen = !isFullScreen;
        NormalLayout.IsVisible = !isFullScreen;
        FullScreenLayout.IsVisible = isFullScreen;
    }

    private void OnPlaylistPlayClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Playlist playlist)
        {
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
                        temp.Tracks.Add(new Track
                        {
                            Path = file,
                            Title = filename,
                            Artist = "Unknown"
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


    private void OnCreatePlaylistClicked(object sender, EventArgs e)
    {
        var newPlaylist = new Playlist
        {
            Name = $"Плейлист {AudioManager.Instance.Playlists.Count + 1}",
            IsExpanded = true
        };

        AudioManager.Instance.Playlists.Add(newPlaylist);
    }

    private void OnTogglePlaylistClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Playlist playlist)
        {
            playlist.IsExpanded = !playlist.IsExpanded;
        }
    }

    private void OnTrackPlayClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Track track)
        {
            var manager = AudioManager.Instance;
            Playlist? parentPlaylist = null;
            foreach(var pl in manager.Playlists)
            {
                if (pl.Tracks.Contains(track))
                {
                    parentPlaylist = pl;
                    break;
                }
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
    }

    private async void OnMediaEnded(object sender, EventArgs e)
    {
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
}