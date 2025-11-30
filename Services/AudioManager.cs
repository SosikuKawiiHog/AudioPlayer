using AudioPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TagLib;

//misha SINGLETOOOON

namespace AudioPlayer.Services
{
    public class AudioManager : INotifyPropertyChanged
    {
        private static AudioManager? _instance;
        public static AudioManager Instance => _instance ??= new AudioManager();

        public ObservableCollection<Playlist> Playlists { get; } = new();
        public ObservableCollection<Track> Queue { get; } = new();

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
                byte[]? coverDataTemp = null;
                string name = fileName;
                string artist = "Unknown";
                try
                {
                    using (var tagFile = TagLib.File.Create(path))
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
                tracks.Add(new Track { Path = path, Title = name, Artist = artist, CoverData = coverDataTemp });
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



    public class ByteArrayToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] imageData && imageData != null && imageData.Length > 0)
            {
                return ImageSource.FromStream(() => new MemoryStream(imageData));
            }

            // Возвращаем изображение-заглушку, если обложки нет
            return ImageSource.FromFile("swag.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
