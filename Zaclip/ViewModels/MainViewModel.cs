using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Zaclip.Command;
using Zaclip.Command.Common;
using Zaclip.Command.Db;
using Zaclip.Db;
using Zaclip.Models;
using Zaclip.Services.ClipboardItemsService;
using Zaclip.Services.ServerClipboardService;
using Zaclip.States;
using Zaclip.ViewModels;
using Zaclip.ViewModels.Controls;

namespace Zaclip.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<ClipboardItem> Items { get; }= new ObservableCollection<ClipboardItem>();
        public bool HasItem => Items.Count > 0;
        public ICommand CloseCommand { get; }
        public ICommand ChangeTabCommand { get; }
        public ICommand PersistCommand { get; }
        public ICommand DeleteCommand { get; }
        public AccountIconViewModel AccountIconViewModel { get; }

        public event Action? RequestClose;
        public event Func<string, bool>? RequestConfirm;
        private IServerClipboardService _serverClipboardService;
        private SessionContext _session;
        private TemporaryClipboardListViewModel _temporaryViewModel;
        private SavedClipboardListViewModel _savedViewModel;
        private IClipboardListAction _currentListViewModel;
        public IClipboardListAction CurrentListViewModel
        {
            get => _currentListViewModel;
            set
            {
                _currentListViewModel = value;
                OnPropertyChanged();
            }
        }


        public MainViewModel(
            TemporaryClipboardListViewModel temporaryViewModel,
            SavedClipboardListViewModel savedViewModel,
            IServerClipboardService serverClipboardService, SessionContext session, AccountIconViewModel accountIconViewModel)
        {
            _serverClipboardService = serverClipboardService;
            _session = session;
            _temporaryViewModel = temporaryViewModel;
            _savedViewModel = savedViewModel;
            AccountIconViewModel = accountIconViewModel;
            CloseCommand = new WindowHideCommand(this);
            ChangeTabCommand = new RelayCommand<string>(execute: (target) =>
            {
                CurrentListViewModel = target == "Temporary" ? _temporaryViewModel : _savedViewModel;
                OnPropertyChanged(nameof(IsTemporaryTab));
                OnPropertyChanged(nameof(IsSavedTab));
            });
            PersistCommand = new PersistClipboardItemCommand();
            DeleteCommand = new RelayCommand<ClipboardItem>(execute: Delete);
            _currentListViewModel = _temporaryViewModel;

            Items.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasItem));
            };
        }

        public Boolean IsLoggedIn => _session.IsLoggedIn;

        public bool IsTemporaryTab =>
            CurrentListViewModel is TemporaryClipboardListViewModel;

        public bool IsSavedTab =>
            CurrentListViewModel is SavedClipboardListViewModel;

        public async Task InitializeAsync()
        {
            await _temporaryViewModel.InitializeAsync();
            await _savedViewModel.InitializeAsync();
        }

        public void WindowHide()
        {
            this.RequestClose?.Invoke();
        }

        public void ResetListPosition()
        {
            _currentListViewModel = _temporaryViewModel;
            
        }


        public async  Task AddTemporaryClipboardItem(string text)
        {
            await _temporaryViewModel.AddItem(text);
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
