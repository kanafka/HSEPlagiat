using Microsoft.AspNetCore.Mvc;

namespace File_Storing_Service;
[ApiController]
[Route("[controller]")]
public class FileController : ControllerBase
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
            return File(stream, contentType, fileName);
        }
        catch (FileNotFoundException)
        {
            // 404, если нет метаданных или файла на диске
            return NotFound(new { code = "FileNotFound", message = "Файл не найден" });
        }
        catch (Exception)
        {
            // 500, если что-то ещё пошло не так
            return StatusCode(500, new { code = "InternalError", message = "Внутренняя ошибка сервиса хранения" });
        }
    }

    [HttpGet("getAllId")]
    public IActionResult GetAllId()
    {
        // тут, как правило, не падает
        var ids = _storage.GetAllIds();
        return Ok(ids);
    }

    [HttpPost("upload")]
    public IActionResult Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { code = "InvalidFile", message = "Файл не был передан или пустой" });

        try
        {
            var id = _storage.SaveFile(file);
            return Ok(new { id, message = "Файл успешно сохранён" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { code = "InternalError", message = "Ошибка при сохранении файла" });
        }
    }
}
