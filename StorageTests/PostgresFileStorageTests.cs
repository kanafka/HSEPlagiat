using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Moq;
using File_Storing_Service;
using System.IO;
using System.Text;

public class PostgresFileStorageTests
{
    private FileDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<FileDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new FileDbContext(options);
    }

    private IFormFile CreateFakeFormFile(string content, string fileName = "test.txt")
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };
    }

    [Fact]
    public void SaveFile_ShouldSaveAndReturnId()
    {
        var db = CreateInMemoryDb();
        var storage = new PostgresFileStorage(db);
        var file = CreateFakeFormFile("hello world");

        var id = storage.SaveFile(file);
        Assert.True(db.Files.Any(f => f.Id == id));
    }

    [Fact]
    public void SaveFile_ShouldReturnSameIdForDuplicate()
    {
        var db = CreateInMemoryDb();
        var storage = new PostgresFileStorage(db);
        var file1 = CreateFakeFormFile("duplicate content");
        var file2 = CreateFakeFormFile("duplicate content");

        var id1 = storage.SaveFile(file1);
        var id2 = storage.SaveFile(file2);

        Assert.Equal(id1, id2);
    }

    [Fact]
    public void GetAllIds_ShouldReturnSavedIds()
    {
        var db = CreateInMemoryDb();
        var storage = new PostgresFileStorage(db);
        var file = CreateFakeFormFile("test data");
        var id = storage.SaveFile(file);

        var ids = storage.GetAllIds();
        Assert.Contains(id, ids);
    }
}