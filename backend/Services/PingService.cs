using Backend.Models;

namespace Backend.Services;

public class PingService : IPingService
{
    public Task<Ping?> PingAsync()
    {
        var ping = new Ping
        {
            Description = "Hello World"
        };
        return Task.FromResult<Ping?>(ping);
    }
}
