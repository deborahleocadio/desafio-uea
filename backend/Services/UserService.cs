using Backend.Models;

namespace Backend.Services;

public class UserService : IUserService
{
    private readonly List<User> _users = [];

    public UserService()
    {
        _users.Add(new User { Id = 1, Name = "João Silva", Email = "joao@email.com" });
        _users.Add(new User { Id = 2, Name = "Maria Santos", Email = "maria@email.com" });
        _users.Add(new User { Id = 3, Name = "Pedro Oliveira", Email = "pedro@email.com" });
    }

    public Task<IEnumerable<User>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<User>>(_users);
    }

    public Task<User?> GetByIdAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }
}
