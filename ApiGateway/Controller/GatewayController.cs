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

    [HttpPost("upload")]
    public IActionResult UploadFile(IFormFile file)
    {
        var client = _httpClientFactory.CreateClient();

        using var memoryStream = new MemoryStream();
        file.CopyTo(memoryStream);
        memoryStream.Position = 0;

        var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(memoryStream);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
        content.Add(fileContent, "file", file.FileName);

        var response = client.PostAsync("http://file-storage:8001/file/upload", content).Result;
        var result = response.Content.ReadAsStringAsync().Result;

        return Content(result, "application/json");
    }

    [HttpGet("file/{fileId:guid}")]
    public IActionResult GetFile(Guid fileId)
    {
        var client = _httpClientFactory.CreateClient();
        var response = client.GetAsync($"http://file-storage:8001/file/get/{fileId}").Result;

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, "Файл не найден или ошибка на стороне file-service");
        }

        var stream = response.Content.ReadAsStreamAsync().Result;
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
        var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "downloaded_file";

        return File(stream, contentType, fileDownloadName: fileName);
    }

    [HttpGet("analyze/{fileId:guid}")]
    public IActionResult Analyze(Guid fileId)
    {
        var client = _httpClientFactory.CreateClient();
        var response = client.GetAsync($"http://file-analysis:8002/analyze/{fileId}").Result;
        var result = response.Content.ReadAsStringAsync().Result;

        return Content(result, "application/json");
    }
}
