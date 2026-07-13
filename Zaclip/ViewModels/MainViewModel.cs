using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Zaclip.Command;
using Zaclip.Command.Common;
using Zaclip.Command.Db;
using Zaclip.Db;
using Zaclip.Models;

namespace Zaclip.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ClipboardItem> Items { get; }= new ObservableCollection<ClipboardItem>();
        public bool HasItem => Items.Count > 0;
        public ICommand CloseCommand { get; }
        public ICommand PersistCommand { get; }
        public ICommand DeleteCommand { get; }

        public event Action? RequestClose;
        public event Func<string, bool>? RequestConfirm;


        public MainViewModel()
        {
            CloseCommand = new WindowHideCommand(this);
            PersistCommand = new PersistClipboardItemCommand();
            DeleteCommand = new RelayCommand<ClipboardItem>(execute: Delete);

            using (var db = new AppDbContext())
            {
                var list = db.ClipItems.OrderByDescending(x => x.CreatedAt).Take(50).ToList();
                foreach (var item in list)
                {
                    Items.Add(item);
                }
            }

            Items.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasItem));
            };
        }

        public void WindowHide()
        {
            this.RequestClose?.Invoke();
        }


        // --- INotifyPropertyChanged ---
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Delete(ClipboardItem? item)
        {
            if (item == null) return;

            if (item.Persisted)
            {
                var result = RequestConfirm?.Invoke("保存済みのアイテムです。\r\n削除しますか？");
                if (result != true) return;
            }
            using (var db = new AppDbContext())
            {
                var target = db.ClipItems.FirstOrDefault(x => x.Id == item.Id);
                if (target != null)
                {
                    db.ClipItems.Remove(target);
                    db.SaveChanges();
                }
            }

            // --- UI更新 ---
            Items.Remove(item);
        }
    }
}
