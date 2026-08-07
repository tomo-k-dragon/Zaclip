using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection.Emit;
using System.Text;
using System.Windows.Input;
using Zaclip.Command;
using Zaclip.Db;
using Zaclip.Models;
using Zaclip.Services.ClipboardEventService;
using Zaclip.Services.ClipboardItemsService;
using Zaclip.Services.LocalClipboardService;
using Zaclip.Services.ServerClipboardService;
using Zaclip.States;

namespace Zaclip.ViewModels
{
    public class SavedClipboardListViewModel : ViewModelBase, IClipboardListAction
    {
        public ObservableCollection<ClipboardItem> Items { get; } = new ObservableCollection<ClipboardItem>();
        public ICommand SaveCommand { get; } = new RelayCommand<ClipboardItem>(execute: item => { /* Implement save logic */ });
        public ICommand DeleteCommand { get; } = new RelayCommand<ClipboardItem>(execute: item => { /* Implement delete logic */ });
        private SessionContext _session;
        private ILocalClipboardService _localClipboardService;
        private IServerClipboardService _serverClipboardService;
        private IClipboardEventService _clipboardEventService;
        public SavedClipboardListViewModel(SessionContext session, IServerClipboardService serverClipboardService, ILocalClipboardService localClipboardService, IClipboardEventService clipboardEventService)
        {
            _session = session;
            _serverClipboardService = serverClipboardService;
            _localClipboardService = localClipboardService;
            _clipboardEventService = clipboardEventService;
        }

        public async Task InitializeAsync()
        {
            var localItems = await getLocalClipboardItemsAsync();
            foreach (var item in localItems)
            {
                Items.Add(item);
            }
            _clipboardEventService.ItemSaved += OnLocalItemSaved;
            if (!_session.IsLoggedIn) return;

            var serverItems = await getServerClipboardItemsAsync();
            foreach (var item in serverItems)
            {
                Items.Add(item);
            }
        }

        private async void OnLocalItemSaved(int itemId)
        {
            var item = await _localClipboardService.GetItemAsync(itemId);
            if(item != null)
                Items.Insert(0, item);
        }

        public bool HasItem => Items.Count > 0;

        private async Task<List<ClipboardItem>> getLocalClipboardItemsAsync()
        {
            var items = await _localClipboardService.GetAsync(new Dtos.ClipboardQuery { Persisted = true });
            return items;
        }

        private async Task<ClipboardItem[]> getServerClipboardItemsAsync()
        {
            var result = await _serverClipboardService.GetClipboardItemsAsync();
            return result.Select(r => new ClipboardItem
            {
                Text = r.Content,
                CreatedAt = r.UpdatedAt
            }).ToArray();
        }
    }
}
