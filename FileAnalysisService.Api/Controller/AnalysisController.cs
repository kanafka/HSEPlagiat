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

    [HttpGet("{fileId:guid}")]
    public IActionResult Analyze(Guid fileId)
    {
        try
        {
            var result = _service.PlagiatAnalyze(fileId);
            return Ok(result);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { code = "AnalysisNotFound", message = "Результаты анализа не найдены" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { code = "InternalError", message = "Ошибка при выполнении анализа" });
        }
    }

    [HttpGet("WordAnalyze/{fileId:guid}")]
    public IActionResult WordAnalyze(Guid fileId)
    {
        try
        {
            var result = _service.WordAnalyze(fileId);
            return Ok(result);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { code = "AnalysisNotFound", message = "Результаты анализа не найдены" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { code = "InternalError", message = "Ошибка при анализе текста" });
        }
    }

    [HttpGet("getWordCloud/{fileId:guid}")]
    public IActionResult GetWordCloud(Guid fileId)
    {
        try
        {
            var (stream, contentType, fileName) = _wordCloudService.GetWordCloud(fileId);
            return File(stream, contentType, fileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { code = "WordCloudNotFound", message = "Облако слов не найдено" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { code = "InternalError", message = "Ошибка при получении облака слов" });
        }
    }
}