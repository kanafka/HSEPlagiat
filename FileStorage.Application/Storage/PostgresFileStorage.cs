using System.Security.Cryptography;
using System.Text;
using File_Storing_Service;

using System.Security.Cryptography;
using File_Storing_Service;
using Microsoft.AspNetCore.Http;

public class PostgresFileStorage : IFileStorage
{
    private readonly string _basePath;
    private readonly FileDbContext _db;

    public PostgresFileStorage(FileDbContext db)
    {
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "Files");
        Directory.CreateDirectory(_basePath);
        _db = db;
    }

    public Guid SaveFile(IFormFile file)
    {

        string hash;
        using (var memoryStream = new MemoryStream())
        {
            file.CopyTo(memoryStream);
            memoryStream.Position = 0;
            using var sha = SHA256.Create();
            hash = BitConverter.ToString(sha.ComputeHash(memoryStream)).Replace("-", "").ToLowerInvariant();
        }


        var existingFile = _db.Files.FirstOrDefault(f => f.Hash == hash);
        if (existingFile != null)
        {
            return existingFile.Id;
        }


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
        Console.WriteLine(file.Location);
        var stream = new FileStream(file.Location, FileMode.Open, FileAccess.Read);
        var contentType = "application/octet-stream";
        return (stream, contentType, file.Name);
    }

    public List<Guid> GetAllIds()
    {
        return _db.Files.Select(f => f.Id).ToList();
    }
}
