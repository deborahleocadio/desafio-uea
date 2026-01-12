using Backend.Models;

namespace Backend.Services;

public interface IPingService
{
    Task<Ping?> PingAsync();
}
