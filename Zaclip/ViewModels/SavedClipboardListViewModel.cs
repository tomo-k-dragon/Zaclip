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
using System.Linq;
using Zaclip.Presentations; // 追加

namespace Zaclip.ViewModels
{
    public class SavedClipboardListViewModel : ViewModelBase, IClipboardListAction
    {
        public ObservableCollection<ClipboardViewItem> Items { get; } = new ObservableCollection<ClipboardViewItem>();
        private List<ClipboardViewItem> localItems = new List<ClipboardViewItem>();
        private List<ClipboardViewItem> serverItems = new List<ClipboardViewItem>();
        public ICommand SaveCommand { get; } = new RelayCommand<ClipboardItem>(execute: item => { /* Implement save logic */ });
        public ICommand DeleteCommand { get; } = new RelayCommand<ClipboardItem>(execute: item => { /* Implement delete logic */ });
        private SessionContext _session;
        private ILocalClipboardService _localClipboardService;
        private IServerClipboardService _serverClipboardService;
        private IClipboardEventService _clipboardEventService;
        private int pageSize = 50;
        private int localItemsSkip = 0;
        private int serverItemsSkip = 0;
        private bool showAllData = false;
        private Dictionary<Guid, SaveDestination> _saveDistDic = new Dictionary<Guid, SaveDestination>();
        public SavedClipboardListViewModel(SessionContext session, IServerClipboardService serverClipboardService, ILocalClipboardService localClipboardService, IClipboardEventService clipboardEventService)
        {
            _session = session;
            _serverClipboardService = serverClipboardService;
            _localClipboardService = localClipboardService;
            _clipboardEventService = clipboardEventService;
            _session.SessionChanged += () => RefreshData();
        }

        private SaveDestination GetSaveState(Guid guid)
        {
            if (_saveDistDic.TryGetValue(guid, out var state))
                return state;

            return SaveDestination.None;
        }

        public async Task InitializeAsync()
        {
            _clipboardEventService.ItemSaved += OnLocalItemSaved;
            await AddSavedItems(pageSize);
        }

        public async void RefreshData()
        {
            Items.Clear();
            localItems.Clear();
            serverItems.Clear();
            localItemsSkip = 0;
            serverItemsSkip = 0;
            showAllData = false;
            _saveDistDic.Clear();
            await AddSavedItems(pageSize);
        }

        /// <summary>
        /// ItemsへaddCount文のアイテムを追加する。ローカルDB、サーバーのデータを全て使い切った場合はaddCountに達していなくても終了
        /// </summary>
        /// <param name="addCount"></param>
        private async Task AddSavedItems(int addCount)
        {
            var initialCount = Items.Count;
            var localEmpty = false;
            var serverEmpty = false;
            while (Items.Count <= initialCount + addCount)
            {
                if (localItems.Count < addCount)
                {
                    var locals = await getLocalClipboardItemsAsync();
                    if (locals.Count < pageSize)
                    {
                        localEmpty = true;
                    }
                    foreach (var item in locals)
                    {
                        localItems.Add(item);
                    }
                }
                if(serverItems.Count < addCount)
                {
                    var servers = await getServerClipboardItemsAsync();
                    if(servers.Count < pageSize)
                    {
                        serverEmpty = true;
                    }
                    foreach (var item in servers)
                    {
                        serverItems.Add(item);
                    }
                }
                if(localItems.Count > 0 || serverItems.Count > 0)
                    MergeSavedItems(addCount, localItems, serverItems, localEmpty, serverEmpty);

                if (localEmpty && serverEmpty)
                {
                    showAllData = true;
                    break;
                }
            }

        }

        private void MergeSavedItems(int addCount, List<ClipboardViewItem> locals, List<ClipboardViewItem> servers, bool localEmpty, bool serverEmpty)
        {
            int added = 0;
            var KeySet = new HashSet<Guid>(Items.Select(i => i.Guid));
            while (added < addCount)
            {
                if ((locals.Count == 0 && !localEmpty) || (servers.Count == 0 && !serverEmpty))
                    break;

                var localItem = locals.Count > 0 ? locals[0] : null;
                var serverItem = servers.Count > 0 ? servers[0] : null;

                if (localItem == null && serverItem == null) return;

                ClipboardViewItem? item;
                if (localItem != null && serverItem != null)
                {
                    item = localItem.UpdatedAt < serverItem.UpdatedAt ? serverItem : localItem;
                    if(localItem.UpdatedAt < serverItem.UpdatedAt)
                    {
                        item = serverItem;
                        servers.Remove(serverItem);
                    }
                    else
                    {
                        item = localItem;
                        locals.Remove(localItem);
                    }
                }
                else if (localItem != null)
                {
                    item = localItem;
                    locals.Remove(item);
                }
                else if (serverItem != null)
                {
                    item = serverItem;
                    servers.Remove(item);
                }else
                {
                    return;
                }

                if(KeySet.Contains(item.Guid))
                    continue;

                Items.Add(item);
                KeySet.Add(item.Guid);
            }
        }

        private async void OnLocalItemSaved(int itemId)
        {
            var item = await _localClipboardService.GetItemAsync(itemId);
            if (item == null)
                return;

            var guids = await _serverClipboardService.GetExistingGuidsAsync(new List<Guid> { item.Guid });
            SaveDestination saveState;
            if(guids.Count > 0 && item.Persisted)
            {
                saveState = SaveDestination.LocalAndCloud;
            }else if(guids.Count > 0)
            {
                saveState = SaveDestination.Cloud;
            }else
            {
                saveState = SaveDestination.Local;
            }
            _saveDistDic.Add(item.Guid, saveState);
            if(item != null)
                Items.Insert(0, new ClipboardViewItem(item, saveState));
        }

        public bool HasItem => Items.Count > 0;

        /// <summary>
        /// ローカルDBからデータを取得して、サーバーにも保存されているかを検証し保存先辞書への登録までを行う。
        /// </summary>
        /// <returns></returns>
        private async Task<List<ClipboardViewItem>> getLocalClipboardItemsAsync()
        {
            var items = await _localClipboardService.GetAsync(new Dtos.ClipboardQuery { Persisted = true, Skip = localItemsSkip, Take = pageSize });
            localItemsSkip += pageSize;
            var localIdList = items.Where((item) => !_saveDistDic.ContainsKey(item.Guid)).Select(x => x.Guid).ToList();
            var serverIdList = await _serverClipboardService.GetExistingGuidsAsync(localIdList);
            foreach(var id in localIdList)
            {
                _saveDistDic.Add(id, serverIdList.Contains(id) ? SaveDestination.LocalAndCloud : SaveDestination.Local);
            }
            return items.Select(x => new ClipboardViewItem(x, _saveDistDic.ContainsKey(x.Guid) ? _saveDistDic[x.Guid] : SaveDestination.None)).ToList();
        }

        /// <summary>
        /// サーバーからClipboardItemを取得して、ローカルにも保存されているかを検証し保存先辞書への登録までを行う。
        /// </summary>
        /// <returns></returns>
        private async Task<List<ClipboardViewItem>> getServerClipboardItemsAsync()
        {
            var result = await _serverClipboardService.GetClipboardItemsAsync(serverItemsSkip, pageSize);
            serverItemsSkip += pageSize;
            var serverItems = result.Select(r => new ClipboardItem
            {
                Guid = r.Guid,
                Content = r.Content,
                UpdatedAt = r.UpdatedAt
            }).ToList();
            var serverIdList = serverItems.Select(x => x.Guid).Where(x => !_saveDistDic.ContainsKey(x)).ToList();
            var localIdList = await _localClipboardService.GetExistGuidsAsync(serverIdList);
            foreach(var id in serverIdList)
            {
                _saveDistDic.Add(id, localIdList.Contains(id) ? SaveDestination.LocalAndCloud : SaveDestination.Cloud);
            }
            return serverItems.Select(x => new ClipboardViewItem(x, _saveDistDic.ContainsKey(x.Guid) ? _saveDistDic[x.Guid] : SaveDestination.None)).ToList();
        }
    }
}
