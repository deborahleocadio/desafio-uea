using Backend.Models;
using Backend.Services;

namespace Backend.Services;

public class TaskService : ITaskService
{
    private readonly List<TaskItem> _tasks = [];
    private int _nextTaskId = 1;

    public TaskService()
    {
        var task1 = new TaskItem
        {
            Id = _nextTaskId++,
            Title = "Definir requisitos do projeto",
            Description = "Levantar e documentar todos os requisitos",
            Status = TaskStatusEnum.Completed,
            AssignedToUserId = 1,
            CompletedAt = DateTime.UtcNow.AddDays(-2)
        };
        _tasks.Add(task1);

        var task2 = new TaskItem
        {
            Id = _nextTaskId++,
            Title = "Criar wireframes",
            Description = "Desenhar protótipos das telas",
            Status = TaskStatusEnum.Completed,
            AssignedToUserId = 2,
            PrerequisiteTaskIds = new List<int> { 1 },
            CompletedAt = DateTime.UtcNow.AddDays(-1)
        };
        _tasks.Add(task2);

        var task3 = new TaskItem
        {
            Id = _nextTaskId++,
            Title = "Implementar backend",
            Description = "Desenvolver API REST",
            Status = TaskStatusEnum.InProgress,
            AssignedToUserId = 1,
            PrerequisiteTaskIds = new List<int> { 1 }
        };
        _tasks.Add(task3);

        var task4 = new TaskItem
        {
            Id = _nextTaskId++,
            Title = "Implementar frontend",
            Description = "Desenvolver interface em Angular",
            Status = TaskStatusEnum.Pending,
            AssignedToUserId = 2,
            PrerequisiteTaskIds = new List<int> { 2, 3 }
        };
        _tasks.Add(task4);

        var task5 = new TaskItem
        {
            Id = _nextTaskId++,
            Title = "Testes integrados",
            Description = "Executar testes end-to-end",
            Status = TaskStatusEnum.Pending,
            PrerequisiteTaskIds = new List<int> { 4 }
        };
        _tasks.Add(task5);
    }

    public Task<IEnumerable<TaskItem>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<TaskItem>>(_tasks);
    }


    public Task<TaskItem?> GetByIdAsync(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        return Task.FromResult(task);
    }

    public Task<TaskItem> CreateAsync(TaskItem task)
    {
        task.Id = _nextTaskId++;
        task.CreatedAt = DateTime.UtcNow;
        task.Status = TaskStatusEnum.Pending;

        task.PrerequisiteTaskIds ??= new List<int>();

        foreach (var prereqId in task.PrerequisiteTaskIds)
        {
            if (!_tasks.Any(t => t.Id == prereqId))
            {
                throw new InvalidOperationException(
                    $"Pré-requisito com ID {prereqId} não encontrado.");
            }
        }

        _tasks.Add(task);

        foreach (var prereqId in task.PrerequisiteTaskIds)
        {
            if (WouldCreateCircularDependency(task.Id, prereqId))
            {
                _tasks.Remove(task);
                throw new InvalidOperationException(
                    $"Não é possível criar tarefa: pré-requisito {prereqId} criaria dependência circular.");
            }
        }

        return Task.FromResult(task);
    }

    public Task<TaskItem?> UpdateStatusAsync(int id, TaskStatusEnum newStatus, int? userId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task == null)
            return Task.FromResult<TaskItem?>(null);

        if (newStatus == TaskStatusEnum.InProgress && userId.HasValue)
        {
            var userHasTaskInProgress = _tasks.Any(t =>
                t.AssignedToUserId == userId &&
                t.Status == TaskStatusEnum.InProgress &&
                t.Id != id);

            if (userHasTaskInProgress)
            {
                throw new InvalidOperationException(
                    "Não é possível iniciar esta tarefa. Você já tem uma tarefa em andamento. " +
                    "Conclua ou pause a tarefa atual antes de iniciar uma nova.");
            }
        }

        if (newStatus == TaskStatusEnum.Completed)
        {
            var incompletedPrereqs = task.PrerequisiteTaskIds
                .Select(prereqId => _tasks.FirstOrDefault(t => t.Id == prereqId))
                .Where(prereq => prereq != null && prereq.Status != TaskStatusEnum.Completed)
                .ToList();

            if (incompletedPrereqs.Any())
            {
                var prereqNames = string.Join(", ", incompletedPrereqs.Select(t => $"'{t!.Title}'"));
                throw new InvalidOperationException(
                    $"Não é possível concluir esta tarefa. " +
                    $"As seguintes tarefas pré-requisitos não estão concluídas: {prereqNames}");
            }

            task.CompletedAt = DateTime.UtcNow;
        }

        task.Status = newStatus;
        if (newStatus == TaskStatusEnum.InProgress && userId.HasValue)
        {
            task.AssignedToUserId = userId;
        }

        return Task.FromResult<TaskItem?>(task);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task == null)
        {
            return Task.FromResult(false);
        }

        var dependentTasks = _tasks
            .Where(t => t.PrerequisiteTaskIds.Contains(id) && t.Id != id)
            .ToList();

        if (dependentTasks.Any())
        {
            var dependentNames = string.Join(", ", dependentTasks.Select(t => $"'{t.Title}'"));
            throw new InvalidOperationException(
                $"Não é possível excluir esta tarefa. As seguintes tarefas dependem dela: {dependentNames}");
        }

        _tasks.Remove(task);
        return Task.FromResult(true);
    }

    public Task<bool> AddPrerequisiteAsync(int taskId, int prerequisiteId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        var prerequisiteTask = _tasks.FirstOrDefault(t => t.Id == prerequisiteId);

        if (task == null || prerequisiteTask == null)
            return Task.FromResult(false);

        if (task.Status == TaskStatusEnum.Completed)
        {
            throw new InvalidOperationException(
                "Não é possível adicionar pré-requisitos a uma tarefa já concluída.");
        }

        if (taskId == prerequisiteId)
        {
            throw new InvalidOperationException(
                "Uma tarefa não pode ser pré-requisito de si mesma.");
        }

        if (WouldCreateCircularDependency(taskId, prerequisiteId))
        {
            throw new InvalidOperationException(
                "Não é possível adicionar este pré-requisito pois criaria uma dependência circular.");
        }

        if (!task.PrerequisiteTaskIds.Contains(prerequisiteId))
        {
            task.PrerequisiteTaskIds.Add(prerequisiteId);
        }

        return Task.FromResult(true);
    }

    public Task<bool> RemovePrerequisiteAsync(int taskId, int prerequisiteId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        var prerequisiteTask = _tasks.FirstOrDefault(t => t.Id == prerequisiteId);

        if (task == null || prerequisiteTask == null)
            return Task.FromResult(false);

        if (task.Status == TaskStatusEnum.Completed)
        {
            throw new InvalidOperationException(
                "Não é possível remover pré-requisitos de uma tarefa já concluída.");
        }

        task.PrerequisiteTaskIds.Remove(prerequisiteId);

        return Task.FromResult(true);
    }

    public Task<IEnumerable<TaskItem>> GetUserTasksAsync(int userId)
    {
        var userTasks = _tasks.Where(t => t.AssignedToUserId == userId);
        return Task.FromResult(userTasks);
    }

    public bool CanStartTask(int taskId, int userId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null)
            return false;

        if (task.Status != TaskStatusEnum.Pending)
            return false;

        var hasIncompletedPrereqs = task.PrerequisiteTaskIds
            .Select(prereqId => _tasks.FirstOrDefault(t => t.Id == prereqId))
            .Any(prereq => prereq == null || prereq.Status != TaskStatusEnum.Completed);

        if (hasIncompletedPrereqs)
            return false;

        var userHasTaskInProgress = _tasks.Any(t =>
            t.AssignedToUserId == userId &&
            t.Status == TaskStatusEnum.InProgress &&
            t.Id != taskId);

        return !userHasTaskInProgress;
    }

    public bool CanCompleteTask(int taskId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null)
            return false;

        var incompletedPrereqs = task.PrerequisiteTaskIds
            .Select(prereqId => _tasks.FirstOrDefault(t => t.Id == prereqId))
            .Where(prereq => prereq != null && prereq.Status != TaskStatusEnum.Completed)
            .ToList();

        return !incompletedPrereqs.Any();
    }

    public List<string> GetBlockingReasons(int taskId, int? userId)
    {
        var reasons = new List<string>();
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);

        if (task == null)
            return reasons;

        var incompletedPrereqs = task.PrerequisiteTaskIds
            .Select(prereqId => _tasks.FirstOrDefault(t => t.Id == prereqId))
            .Where(prereq => prereq != null && prereq.Status != TaskStatusEnum.Completed)
            .ToList();

        if (incompletedPrereqs.Any())
        {
            var prereqNames = string.Join(", ", incompletedPrereqs.Select(t => $"'{t!.Title}'"));
            reasons.Add($"Aguardando conclusão de: {prereqNames}");
        }

        if (userId.HasValue)
        {
            var userTaskInProgress = _tasks.FirstOrDefault(t =>
                t.AssignedToUserId == userId &&
                t.Status == TaskStatusEnum.InProgress &&
                t.Id != taskId);

            if (userTaskInProgress != null)
            {
                reasons.Add($"Você já está trabalhando em: '{userTaskInProgress.Title}'");
            }
        }

        return reasons;
    }

    private bool WouldCreateCircularDependency(int taskId, int prerequisiteId)
    {
        // Verifica se adicionar prerequisiteId como pré-requisito de taskId
        // criaria um ciclo (ex: A depende de B, B depende de C, C depende de A)
        var visited = new HashSet<int>();
        return HasPathTo(prerequisiteId, taskId, visited);
    }

    private bool HasPathTo(int fromId, int toId, HashSet<int> visited)
    {
        if (fromId == toId)
            return true;

        if (visited.Contains(fromId))
            return false;

        visited.Add(fromId);

        var fromTask = _tasks.FirstOrDefault(t => t.Id == fromId);
        if (fromTask == null)
            return false;

        foreach (var prereqId in fromTask.PrerequisiteTaskIds)
        {
            if (HasPathTo(prereqId, toId, visited))
                return true;
        }

        return false;
    }
}
