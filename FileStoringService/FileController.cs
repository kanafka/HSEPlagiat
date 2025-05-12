using Microsoft.AspNetCore.Mvc;

namespace File_Storing_Service;
[ApiController]
[Route("[controller]")]
public class FileController:ControllerBase
{
    private readonly IFileStorage _storage;

    public FileController(IFileStorage storage)
    {
        _storage = storage;
    }

    [HttpGet("get/{id:guid}")]
    public IActionResult Get(Guid id)
    {
        try
        {
            var (stream, contentType, fileName) = _storage.GetFile(id);
            var result = new FileStreamResult(stream, contentType)
            {
                FileDownloadName = fileName
            };
            return result;
        }
        catch (FileNotFoundException)
        {
            return NotFound("Файл не найден");
        }
    }

    [HttpPost("upload")]
    public IActionResult Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Файл не загружен");

        var id = _storage.SaveFile(file);

        return Ok(new
        {
            id,
            message = "Файл успешно сохранён"
        });
    }
}