using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController(ITaskService taskService) : ControllerBase
{
    private readonly ITaskService _taskService = taskService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetAll()
    {
        var tasks = await _taskService.GetAllAsync();
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetById(int id)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task == null)
            return NotFound(new { message = $"Tarefa com ID {id} não encontrada" });

        return Ok(task);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetUserTasks(int userId)
    {
        var tasks = await _taskService.GetUserTasksAsync(userId);
        return Ok(tasks);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> Create([FromBody] TaskItem task)
    {
        if (string.IsNullOrWhiteSpace(task.Title))
            return BadRequest(new { message = "Título da tarefa é obrigatório" });

        var createdTask = await _taskService.CreateAsync(task);
        return CreatedAtAction(nameof(GetById), new { id = createdTask.Id }, createdTask);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<TaskItem>> UpdateStatus(
        int id,
        [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var updatedTask = await _taskService.UpdateStatusAsync(id, request.Status, request.UserId);
            if (updatedTask == null)
                return NotFound(new { message = $"Tarefa com ID {id} não encontrada" });

            return Ok(updatedTask);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/prerequisites/{prereqId}")]
    public async Task<ActionResult> AddPrerequisite(
        int id,
        int prereqId)
    {
        try
        {
            var success = await _taskService.AddPrerequisiteAsync(id, prereqId);
            if (!success)
                return NotFound(new { message = "Tarefa ou pré-requisito não encontrado" });

            return Ok(new { message = "Pré-requisito adicionado com sucesso" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}/prerequisites/{prereqId}")]
    public async Task<ActionResult> RemovePrerequisite(int id, int prereqId)
    {
        var success = await _taskService.RemovePrerequisiteAsync(id, prereqId);
        if (!success)
            return NotFound(new { message = "Tarefa ou pré-requisito não encontrado" });

        return Ok(new { message = "Pré-requisito removido com sucesso" });
    }

    [HttpGet("{id}/can-start")]
    public ActionResult<bool> CanStart(int id, [FromQuery] int userId)
    {
        var canStart = _taskService.CanStartTask(id, userId);
        var reasons = _taskService.GetBlockingReasons(id, userId);

        return Ok(new { canStart, blockingReasons = reasons });
    }

    [HttpGet("{id}/can-complete")]
    public ActionResult<bool> CanComplete(int id)
    {
        var canComplete = _taskService.CanCompleteTask(id);
        var reasons = _taskService.GetBlockingReasons(id, null);

        return Ok(new { canComplete, blockingReasons = reasons });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var success = await _taskService.DeleteAsync(id);
        if (!success)
            return NotFound(new { message = $"Tarefa com ID {id} não encontrada" });

        return NoContent();
    }
}

