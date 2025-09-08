using Models;

namespace Services;

public interface IMyRepository
{
    Task<Item?> GetByIdAsync(int id);
    Task<IEnumerable<Item>> GetAllAsync();
    Task<Item> AddAsync(Item item);
    Task<Item> UpdateAsync(Item item);
    Task<bool> DeleteAsync(int id);
}
