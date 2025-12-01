using AudioPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TagLib;
using File = System.IO.File;

//misha SINGLETOOOON

namespace AudioPlayer.Services
{
    public interface IDataService
    {
        Task SavePlaylistsAsync(IEnumerable<Playlist> playlists);
        Task<List<Playlist>> LoadPlaylistsAsync();
    }

    public class FileDataService : IDataService
    {
        private readonly string _filePath;

        public FileDataService() 
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, "playlists.json");
        }

        public async Task SavePlaylistsAsync(IEnumerable<Playlist> playlists)
        {
            try
            {
                // Исключаем временные плейлисты из сохранения
                var permanentPlaylists = playlists.Where(p => !p.IsTemporary).ToList();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(permanentPlaylists, options);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения: {ex.Message}");
            }
        }

        public async Task<List<Playlist>> LoadPlaylistsAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    System.Diagnostics.Debug.WriteLine("Файл плейлистов не найден");
                    return new List<Playlist>();
                }

                var json = await File.ReadAllTextAsync(_filePath);
                System.Diagnostics.Debug.WriteLine($"SAERMO: {json}");

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };

                var playlists = JsonSerializer.Deserialize<List<Playlist>>(json, options) ?? new List<Playlist>();

                System.Diagnostics.Debug.WriteLine($"SAERMO загружено: {playlists.Count}");

                // Восстанавливаем ObservableCollection для каждого плейлиста
                foreach (var playlist in playlists)
                {
                    System.Diagnostics.Debug.WriteLine($"SAERMO Плейлист: '{playlist.Name}', Треков: {playlist.Tracks?.Count ?? 0}, IsTemporary: {playlist.IsTemporary}");

                    // Убеждаемся, что коллекция инициализирована
                    playlist.Tracks ??= new ObservableCollection<Track>();

                    // Восстанавливаем состояние IsExpanded (по умолчанию true)
                    playlist.IsExpanded = false;
                }

                return playlists;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки: {ex.Message}");
                return new List<Playlist>();
            }
        }
    }
    public class AudioManager : INotifyPropertyChanged
    {
        private static AudioManager? _instance;
        private readonly IDataService _dataService;
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

        private async void LoadPlaylistsOnStartup()
        {
            var savedPlaylists = await _dataService.LoadPlaylistsAsync();

            
            foreach (var playlist in savedPlaylists)
            {
                Playlists.Add(playlist);

                // Подписываемся на изменения каждого плейлиста
                playlist.PropertyChanged += OnPlaylistPropertyChanged;
                if (playlist.Tracks != null)
                {
                    playlist.Tracks.CollectionChanged += OnTracksCollectionChanged;
                }
            }
        }

        private async void OnPlaylistsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await SavePlaylistsAsync();

            // Подписываемся на новые плейлисты
            if (e.NewItems != null)
            {
                foreach (Playlist playlist in e.NewItems)
                {
                    playlist.PropertyChanged += OnPlaylistPropertyChanged;
                    if (playlist.Tracks != null)
                    {
                        playlist.Tracks.CollectionChanged += OnTracksCollectionChanged;
                    }
                }
            }

            // Отписываемся от удаленных плейлистов
            if (e.OldItems != null)
            {
                foreach (Playlist playlist in e.OldItems)
                {
                    playlist.PropertyChanged -= OnPlaylistPropertyChanged;
                    if (playlist.Tracks != null)
                    {
                        playlist.Tracks.CollectionChanged -= OnTracksCollectionChanged;
                    }
                }
            }
        }

        private async void OnPlaylistPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Сохраняем при изменении любого свойства плейлиста
            if (e.PropertyName == nameof(Playlist.Name))
            {
                await SavePlaylistsAsync();
            }
        }

        private async void OnTracksCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
            await SavePlaylistsAsync();
        }

        private async Task SavePlaylistsAsync()
        {
            await _dataService.SavePlaylistsAsync(Playlists);
        }
        private AudioManager()
        {
            _dataService = new FileDataService();
            Playlists.CollectionChanged += OnPlaylistsChanged;
            LoadPlaylistsOnStartup();
        }
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
