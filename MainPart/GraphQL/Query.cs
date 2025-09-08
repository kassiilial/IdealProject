using Services;
using Models;

namespace GraphQL;

public class Query
{
    public async Task<IEnumerable<Item>> GetItems([Service] IMyRepository repo)
    {
        return await repo.GetAllAsync();
    }

    public async Task<Item?> GetItem(int id, [Service] IMyRepository repo)
    {
        return await repo.GetByIdAsync(id);
    }
}
