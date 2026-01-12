namespace Backend.Models;

public class UpdateStatusRequest
{
    public TaskStatusEnum Status { get; set; }
    public int? UserId { get; set; }
}
