using Microsoft.AspNetCore.Mvc;

namespace FileAnalisysService;

[ApiController]
[Route("analyze")]
public class AnalysisController : ControllerBase
{
    private readonly IAnalysisService _service;

    public AnalysisController(IAnalysisService service)
    {
        _service = service;
    }

    [HttpGet("{fileId:guid}")]
    public IActionResult Analyze(Guid fileId)
    {
        try
        {
            var result = _service.Analyze(fileId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}