using Services;
using Models;

namespace GraphQL;

public class Mutation
{
    public async Task<Item> AddItem(Item input, [Service] IMyRepository repo)
    {
        return await repo.AddAsync(input);
    }

    public async Task<Item> UpdateItem(Item input, [Service] IMyRepository repo)
    {
        return await repo.UpdateAsync(input);
    }

    public async Task<bool> DeleteItem(int id, [Service] IMyRepository repo)
    {
        return await repo.DeleteAsync(id);
    }
}
