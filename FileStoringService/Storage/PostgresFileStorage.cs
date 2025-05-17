using System.Security.Cryptography;
using System.Text;
using File_Storing_Service;

using System.Security.Cryptography;
using File_Storing_Service;

public class PostgresFileStorage : IFileStorage
{
    private readonly string _basePath;
    private readonly FileDbContext _db;

    public PostgresFileStorage(IWebHostEnvironment env, FileDbContext db)
    {
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "Files");
        Directory.CreateDirectory(_basePath);
        _db = db;
    }

    public Guid SaveFile(IFormFile file)
    {
        // Считаем хэш сразу из потока без сохранения на диск
        string hash;
        using (var memoryStream = new MemoryStream())
        {
            file.CopyTo(memoryStream);
            memoryStream.Position = 0;
            using var sha = SHA256.Create();
            hash = BitConverter.ToString(sha.ComputeHash(memoryStream)).Replace("-", "").ToLowerInvariant();
        }

        // Проверяем, есть ли файл с таким хэшем
        var existingFile = _db.Files.FirstOrDefault(f => f.Hash == hash);
        if (existingFile != null)
        {
            return existingFile.Id;
        }

        // Если такого файла нет — сохраняем файл на диск
        var id = Guid.NewGuid();
        var filePath = Path.Combine(_basePath, id.ToString());

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        var metadata = new FileMetadata
        {
            Id = id,
            Name = file.FileName,
            Hash = hash,
            Location = filePath
        };

        _db.Files.Add(metadata);
        _db.SaveChanges();

        return id;
    }


    public (Stream stream, string contentType, string fileName) GetFile(Guid id)
    {
        var file = _db.Files.Find(id);
        if (file == null || !System.IO.File.Exists(file.Location))
            throw new FileNotFoundException();

        var stream = new FileStream(file.Location, FileMode.Open, FileAccess.Read);
        var contentType = "application/octet-stream";
        return (stream, contentType, file.Name);
    }

    public List<Guid> GetAllIds()
    {
        return _db.Files.Select(f => f.Id).ToList();
    }
}
