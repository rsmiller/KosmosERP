using Microsoft.AspNetCore.Mvc;

namespace KosmosERP.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(ILogger<DiagnosticsController> logger)
    {
        _logger = logger;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    [HttpPost("test-binding")]
    public IActionResult TestBinding([FromBody] TestModel model)
    {
        _logger.LogInformation("TestBinding called with: {@Model}", model);
        
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Model validation failed: {@Errors}", ModelState);
            return BadRequest(ModelState);
        }

        return Ok(new
        {
            message = "Binding successful",
            receivedModel = model,
            modelState = ModelState.IsValid
        });
    }

    [HttpGet("test-simple")]
    public IActionResult TestSimple()
    {
        _logger.LogInformation("TestSimple endpoint called successfully");
        return Ok(new { message = "Simple GET works" });
    }

    public class TestModel
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}
