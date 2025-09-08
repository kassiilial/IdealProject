using Models;

namespace Services;

public class SqlRepository : IMyRepository
{
    private readonly string _connectionString;
    public SqlRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Task<Item> AddAsync(Item item)
    {
    // TODO: implement real DB persistence (Dapper/ADO.NET/EF Core as desired)
    throw new NotImplementedException("SqlRepository.AddAsync is not implemented. Replace with your DB logic.");
    }

    public Task<IEnumerable<Item>> GetAllAsync()
    {
        throw new NotImplementedException("SqlRepository.GetAllAsync is not implemented. Replace with your DB logic.");
    }

    public Task<Item?> GetByIdAsync(int id)
    {
        throw new NotImplementedException("SqlRepository.GetByIdAsync is not implemented. Replace with your DB logic.");
    }

    public Task<Item> UpdateAsync(Item item)
    {
        throw new NotImplementedException("SqlRepository.UpdateAsync is not implemented. Replace with your DB logic.");
    }

    public Task<bool> DeleteAsync(int id)
    {
        throw new NotImplementedException("SqlRepository.DeleteAsync is not implemented. Replace with your DB logic.");
    }
}
