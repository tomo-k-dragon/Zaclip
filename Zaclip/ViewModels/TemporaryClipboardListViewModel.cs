using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Zaclip.Command;
using Zaclip.Db;
using Zaclip.Dtos;
using Zaclip.Models;
using Zaclip.Services.ClipboardEventService;
using Zaclip.Services.LocalClipboardService;
using Zaclip.States;

namespace Zaclip.ViewModels
{
    public class TemporaryClipboardListViewModel : ViewModelBase, IClipboardListAction
    {
        
        public ObservableCollection<ClipboardItem> Items { get; } = new ObservableCollection<ClipboardItem>();
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        private ILocalClipboardService _localClipboardService;
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

        public TemporaryClipboardListViewModel(ILocalClipboardService localClipboardService, IClipboardEventService clipboardEventService)
        {
            _localClipboardService = localClipboardService;
            _clipboardEventService = clipboardEventService;
            Items.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasItem));
            };
            SaveCommand = new RelayCommand<ClipboardItem>(async (item) => await PersistAsync(item));
            DeleteCommand = new RelayCommand<ClipboardItem>(async (item) => await DeleteAsync(item));
        }
        
        public async Task InitializeAsync()
        {
            var query = new ClipboardQuery { Persisted = false, Take = 50 };
            var itemList = await _localClipboardService.GetAsync(query);
            foreach (var item in itemList)
            {
                Items.Add(item);
            }
        }

        public async Task AddItem(string text)
        {
            var newItem = await _localClipboardService.SaveTemporaryAsync(text);
            Items.Insert(0, newItem);
        } 

        public bool HasItem => Items.Count > 0;

        private async Task PersistAsync(ClipboardItem? item)
        {
            if (item == null) return;

            await _localClipboardService.PersistAsync(item.Id);

            _clipboardEventService.RaiseItemSaved(item.Id);
            Items.Remove(item);
        }

        private async Task DeleteAsync(ClipboardItem? item)
        {
            if (item == null) return;
            await _localClipboardService.DeleteAsync(item.Id);
            Items.Remove(item);
        }
    }
}
