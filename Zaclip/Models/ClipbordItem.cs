using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Zaclip.Models
{
    public class ClipboardItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        private string _text;
        public required string Text 
        { 
            get => _text;
            set {
                if (_text == value) return;
                _text = value;
                OnPropertyChanged(nameof(Text));
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
