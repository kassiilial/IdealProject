using Models;
using Data;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class EfRepository : IMyRepository
{
    private readonly AppDbContext _db;

    public EfRepository(AppDbContext db)
    {
        _db = db;
        _db.Database.EnsureCreated();
    }

    public async Task<Item> AddAsync(Item item)
    {
        var ent = (await _db.Items.AddAsync(item)).Entity;
        await _db.SaveChangesAsync();
        return ent;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var it = await _db.Items.FindAsync(id);
        if (it == null) return false;
        _db.Items.Remove(it);
        await _db.SaveChangesAsync();
        return true;
    }

    public Task<IEnumerable<Item>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Item>>(_db.Items.AsNoTracking().ToList());
    }

    public Task<Item?> GetByIdAsync(int id)
    {
        return _db.Items.FindAsync(id).AsTask();
    }

    public async Task<Item> UpdateAsync(Item item)
    {
        _db.Items.Update(item);
        await _db.SaveChangesAsync();
        return item;
    }
}
