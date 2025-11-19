using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AudioPlayer.Models
{
    public class Playlist : INotifyPropertyChanged
    {
        public string Name { get; set; }

        //этот прикол нужен для связи с ui zov'a ИБО КАК ЯНДЕКС ВЫДАЛ, РЕАЛИЗУЕТ INotifyCollectionChanged
        public ObservableCollection<Track> Tracks { get; set; } = new();
        public bool IsTemporary { get; set; } = false;
        private bool _isExpanded { get; set; } = true;
        public bool IsExpanded { get => _isExpanded; 
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
