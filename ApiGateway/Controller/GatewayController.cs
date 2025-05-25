using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace ApiGateway;

[ApiController]
[Route("[controller]")]
public class GatewayController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public GatewayController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient Client() => _httpClientFactory.CreateClient();

    [HttpPost("upload")]
    public IActionResult UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { code = "InvalidFile", message = "Файл не передан" });

        try
        {
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            ms.Position = 0;

            var content = new MultipartFormDataContent
            {
                { new StreamContent(ms) { Headers = { ContentType = MediaTypeHeaderValue.Parse(file.ContentType) } },
                  "file", file.FileName }
            };

            var resp = Client().PostAsync("http://file-storage:8001/file/upload", content).GetAwaiter().GetResult();
            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode,
                    new { code = "UpstreamError", message = "Ошибка на стороне file-storage" });

            var json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return Content(json, "application/json");
        }
        catch (Exception)
        {
            return StatusCode(502, new { code = "UpstreamUnavailable", message = "Сервис хранения недоступен" });
        }
    }

    [HttpGet("file/{fileId:guid}")]
    public IActionResult GetFile(Guid fileId)
    {
        try
        {
            var resp = Client().GetAsync($"http://file-storage:8001/file/get/{fileId}").GetAwaiter().GetResult();
            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode,
                    new { code = "FileNotFound", message = "Файл не найден или ошибка file-storage" });

            var stream = resp.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
            var ct     = resp.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            var fn     = resp.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "downloaded_file";
            return File(stream, ct, fn);
        }
        catch (Exception)
        {
            return StatusCode(502, new { code = "UpstreamUnavailable", message = "Сервис хранения недоступен" });
        }
    }

    [HttpGet("WorldCloud/{fileId:guid}")]
    public IActionResult GetWorldCloud(Guid fileId)
    {
        try
        {
            var resp = Client().GetAsync($"http://file-analysis:8002/analyze/getWordCloud/{fileId}")
                                .GetAwaiter().GetResult();
            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode,
                    new { code = "WordCloudNotFound", message = "Облако слов не найдено или service unavailable" });

            var stream = resp.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
            var ct     = resp.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            var fn     = resp.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "downloaded_file";
            return File(stream, ct, fn);
        }
        catch (Exception)
        {
            return StatusCode(502, new { code = "UpstreamUnavailable", message = "Сервис анализа недоступен" });
        }
    }

    [HttpGet("analyze/{fileId:guid}")]
    public IActionResult Analyze(Guid fileId)
    {
        try
        {
            var resp = Client().GetAsync($"http://file-analysis:8002/analyze/{fileId}")
                                .GetAwaiter().GetResult();
            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode,
                    new { code = "AnalysisError", message = "Ошибка на стороне analizator" });

            var json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return Content(json, "application/json");
        }
        catch (Exception)
        {
            return StatusCode(502, new { code = "UpstreamUnavailable", message = "Сервис анализа недоступен" });
        }
    }

    [HttpGet("WordAnalyze/{fileId:guid}")]
    public IActionResult WordAnalyze(Guid fileId)
    {
        try
        {
            var resp = Client().GetAsync($"http://file-analysis:8002/analyze/WordAnalyze/{fileId}")
                                .GetAwaiter().GetResult();
            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode,
                    new { code = "AnalysisError", message = "Ошибка на стороне analizator" });

            var json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return Content(json, "application/json");
        }
        catch (Exception)
        {
            return StatusCode(502, new { code = "UpstreamUnavailable", message = "Сервис анализа недоступен" });
        }
    }
}