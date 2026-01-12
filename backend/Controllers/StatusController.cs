using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<object>> GetAll()
    {
        var statuses = new[]
        {
            new { value = 0, label = "Pendente", key = "Pending" },
            new { value = 1, label = "Em Andamento", key = "InProgress" },
            new { value = 2, label = "Concluída", key = "Completed" }
        };

        return Ok(statuses);
    }
}
