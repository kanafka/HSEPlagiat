using Xunit;
using Microsoft.EntityFrameworkCore;
using File_Storing_Service;

public class FileDbContextTests
{
    [Fact]
    public void CanInsertAndRetrieveMetadata()
    {
        var options = new DbContextOptionsBuilder<FileDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using (var context = new FileDbContext(options))
        {
            var file = new FileMetadata
            {
                Id = Guid.NewGuid(),
                Name = "test.txt",
                Hash = "123abc",
                Location = "/path/to/file"
            };
            context.Files.Add(file);
            context.SaveChanges();
        }

        using (var context = new FileDbContext(options))
        {
            var file = context.Files.First();
            Assert.Equal("test.txt", file.Name);
        }
    }
}