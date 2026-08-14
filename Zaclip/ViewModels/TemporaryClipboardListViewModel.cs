using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Zaclip.Command;
using Zaclip.Db;
using Zaclip.Dtos;
using Zaclip.Models;
using Zaclip.Presentations;
using Zaclip.Services.ClipboardEventService;
using Zaclip.Services.ClipboardItemsService;
using Zaclip.Services.LocalClipboardService;
using Zaclip.States;

namespace Zaclip.ViewModels
{
    public class TemporaryClipboardListViewModel : ViewModelBase, IClipboardListAction
    {
        
        public ObservableCollection<ClipboardViewItem> Items { get; } = new ObservableCollection<ClipboardViewItem>();
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        private ILocalClipboardService _localClipboardService;
        private IServerClipboardService _serverClipboardService;
        private IClipboardEventService _clipboardEventService;
        private SaveDestination _selectedSaveDestination;
        public SaveDestination SelectedSaveDestination
        {
            get => _selectedSaveDestination;
            set
            {
                if (_selectedSaveDestination != value)
                {
                    _selectedSaveDestination = value;
                    OnPropertyChanged();
                }
            }
        }

        public TemporaryClipboardListViewModel(ILocalClipboardService localClipboardService, IClipboardEventService clipboardEventService, IServerClipboardService serverClipboardService)
        {
            _localClipboardService = localClipboardService;
            _clipboardEventService = clipboardEventService;
            _serverClipboardService = serverClipboardService;
            Items.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasItem));
            };
            SaveCommand = new RelayCommand<ClipboardViewItem>(async (item) => await PersistAsync(item));
            DeleteCommand = new RelayCommand<ClipboardViewItem>(async (item) => await DeleteAsync(item));
        }
        
        public async Task InitializeAsync()
        {
            var query = new ClipboardQuery { Persisted = false, Take = 50 };
            var itemList = await _localClipboardService.GetAsync(query);
            foreach (var item in itemList)
            {
                Items.Add(new ClipboardViewItem(item, SaveDestination.None));
            }
        }

        public async Task AddItem(string text)
        {
            var newItem = await _localClipboardService.SaveTemporaryAsync(text);
            Items.Insert(0, new ClipboardViewItem(newItem, SaveDestination.None));
        } 

        public bool HasItem => Items.Count > 0;

        private async Task PersistAsync(ClipboardViewItem? item)
        {
            if (item == null) return;

            if(SelectedSaveDestination == SaveDestination.Local || SelectedSaveDestination == SaveDestination.LocalAndCloud)
                await _localClipboardService.PersistAsync(item.Id);

            if(SelectedSaveDestination == SaveDestination.Cloud || SelectedSaveDestination == SaveDestination.LocalAndCloud)
                await _serverClipboardService.CreateClipboardItemAsync(item.Id);

            _clipboardEventService.RaiseItemSaved(item.Id);
            Items.Remove(item);
        }

        private async Task DeleteAsync(ClipboardViewItem? item)
        {
            if (item == null) return;
            await _localClipboardService.DeleteAsync(item.Id);
            Items.Remove(item);
        }
    }
}
