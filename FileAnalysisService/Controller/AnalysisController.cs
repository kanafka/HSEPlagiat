using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace FileAnalisysService;

[ApiController]
[Route("analyze")]
public class AnalysisController : ControllerBase
{
    private readonly IAnalysisService _service;
    private readonly IWordCloudService _wordCloudService;

    public AnalysisController(IAnalysisService service, IWordCloudService wordCloudService)
    {
        _service = service;
        _wordCloudService = wordCloudService;
    }

    [HttpGet("getWordCloud/{fileId:guid}")]
    public IActionResult GetWordCloud(Guid fileId)
    {
        try
        {
            var (stream, contentType, fileName) = _wordCloudService.GetWordCloud(fileId);
            var result = new FileStreamResult(stream, contentType)
            {
                FileDownloadName = fileName
            };
            Console.WriteLine("api konec Get WordCloud");
            return result;
        }
        catch (FileNotFoundException)
        {
            return NotFound("Файл не найден");
        }
    }
    
    [HttpGet("{fileId:guid}")]
    public IActionResult Analyze(Guid fileId)
    {
        try
        {
            var result = _service.PlagiatAnalyze(fileId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpGet("WordAnalyze/{fileId:guid}")]
    public IActionResult WordAnalyze(Guid fileId)
    {
        try
        {
            var result = _service.WordAnalyze(fileId);
            Console.WriteLine($"Controller received: {JsonSerializer.Serialize(result)}");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}