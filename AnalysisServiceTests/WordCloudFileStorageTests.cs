using Xunit;
using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using FileAnalisysService.Storage;
using FileAnalisysService;

public class WordCloudFileStorageTests
{
    [Fact]
    public void Save_And_Get_ShouldWorkCorrectly()
    {
        // Arrange
        var db = new AnalysisDbContext(new DbContextOptionsBuilder<AnalysisDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        var storage = new WordCloudFileStorage(db);

        var fileId = Guid.NewGuid();
        var bytes = new byte[] { 0x1, 0x2, 0x3 };

        // Act
        storage.Save(bytes, fileId);
        var (stream, contentType, fileName) = storage.Get(fileId);

        // Assert
        Assert.NotNull(stream);
        Assert.Equal("application/octet-stream", contentType);
        Assert.Equal("image.png", fileName);
    }

    [Fact]
    public void Get_ShouldThrowIfFileNotFound()
    {
        var db = new AnalysisDbContext(new DbContextOptionsBuilder<AnalysisDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        var storage = new WordCloudFileStorage(db);

        Assert.Throws<FileNotFoundException>(() => storage.Get(Guid.NewGuid()));
    }
}