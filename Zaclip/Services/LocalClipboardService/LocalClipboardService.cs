using Microsoft.EntityFrameworkCore;
using Zaclip.Db;
using Zaclip.Dtos;
using Zaclip.Models;

namespace Zaclip.Services.LocalClipboardService;

public class LocalClipboardService : ILocalClipboardService
{
    private AppDbContext _db;
    public LocalClipboardService(AppDbContext db)
    {
        _db = db;
    }
    public Task<List<ClipboardItem>> GetAsync(ClipboardQuery query)
    {
        var items = _db.ClipItems.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Keyword))
            items = items.Where(x => x.Text.Contains(query.Keyword));

        if (query.Persisted.HasValue)
            items = items.Where(x => x.Persisted == query.Persisted.Value);

        return items
            .OrderByDescending(x => x.CreatedAt)
            .Skip(query.Skip)
            .Take(query.Take)
            .ToListAsync();
    }

    public Task<ClipboardItem?> GetItemAsync(int itemId) =>
        _db.ClipItems.FirstOrDefaultAsync(x => x.Id == itemId);

    public async Task<ClipboardItem> SaveTemporaryAsync(string itemText)
    {
        var item = new ClipboardItem
        {
            Text = itemText,
            CreatedAt = DateTime.Now
        };
        _db.ClipItems.Add(item);
        await _db.SaveChangesAsync();

        return item;
    }

    public async Task PersistAsync(int itemId)
    {
        var item = await _db.ClipItems
            .FirstOrDefaultAsync(x => x.Id == itemId);

        if (item == null)
            return;

        item.Persisted = true;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int itemId)
    {
        var item = await _db.ClipItems
            .FirstOrDefaultAsync(x => x.Id == itemId);

        if (item == null)
            return;

        _db.ClipItems.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteTemporaryAsync()
    {
        var items = await _db.ClipItems
            .Where(x => !x.Persisted)
            .ToListAsync();
        _db.ClipItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }
}