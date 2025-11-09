using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using AudioPlayer.Models;

//misha SINGLETOOOON

namespace AudioPlayer.Services
{
    public class AudioManager : INotifyPropertyChanged
    {
        private static AudioManager? _instance;
        public static AudioManager Instance => _instance ??= new AudioManager();

        public ObservableCollection<Playlist> Playlists { get; } = new();
        public ObservableCollection<Track> Queue { get;  } = new();

        private Playlist? _tempPlaylist;
        public Playlist? TempPlaylist
        {
            get => _tempPlaylist;
            set
            {
                _tempPlaylist = value;
                OnPropertyChanged();
            }
        }

        private Track? _currentTrack;
        public Track? CurrentTrack
        {
            get => _currentTrack;
            set
            {
                _currentTrack = value;
                OnPropertyChanged();
            }
        }

        public bool IsPlaying { get; set; }

        public void LoadTracksFromPaths(IEnumerable<string> paths)
        {
            var tracks = new ObservableCollection<Track>();
            foreach (var path in paths)
            {
                var fileName = Path.GetFileNameWithoutExtension(path);

                tracks.Add(new Track { Path = path, Title = fileName, Artist = "Unknown" });
            }

            TempPlaylist = new Playlist { Name = "Temp", Tracks = tracks, IsTemporary = true };

            Playlists.Add(TempPlaylist);
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
