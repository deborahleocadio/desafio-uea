using Backend.Models;

namespace Backend.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(int id);
    Task<TaskItem> CreateAsync(TaskItem task);
    Task<TaskItem?> UpdateStatusAsync(int id, TaskStatusEnum newStatus, int? userId);
    Task<bool> DeleteAsync(int id);
    Task<bool> AddPrerequisiteAsync(int taskId, int prerequisiteId);
    Task<bool> RemovePrerequisiteAsync(int taskId, int prerequisiteId);
    Task<IEnumerable<TaskItem>> GetUserTasksAsync(int userId);
    bool CanStartTask(int taskId, int userId);
    bool CanCompleteTask(int taskId);
    List<string> GetBlockingReasons(int taskId, int? userId);
}
