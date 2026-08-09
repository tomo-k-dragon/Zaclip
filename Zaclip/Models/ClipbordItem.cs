using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Zaclip.Models
{
    public class ClipboardItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        private string _content;
        public required string Content 
        { 
            get => _content;
            set {
                if (_content == value) return;
                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        private bool _persisted;
        public bool Persisted 
        {
            get => _persisted;
            set 
            { 
                if (_persisted == value) return;
                _persisted = value;
                OnPropertyChanged(nameof(Persisted));
            }
        }
        public DateTime CreatedAt { get; set; }
        public System.Guid Guid { get; set; }
        public DateTime UpdatedAt { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
