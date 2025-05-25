using Microsoft.AspNetCore.Http;

namespace File_Storing_Service;

public interface IFileStorage
{
    Guid SaveFile(IFormFile file);
    (Stream stream, string contentType, string fileName) GetFile(Guid id);
    List<Guid> GetAllIds();
}
