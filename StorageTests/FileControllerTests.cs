using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using File_Storing_Service;
using System.Text;
using System.IO;

public class FileControllerTests
{
    [Fact]
    public void Upload_ShouldReturnOkWithId()
    {
        var mockStorage = new Mock<IFileStorage>();
        mockStorage.Setup(s => s.SaveFile(It.IsAny<IFormFile>())).Returns(Guid.NewGuid());

        var controller = new FileController(mockStorage.Object);
        var file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("test")), 0, 4, "file", "test.txt");

        var result = controller.Upload(file) as OkObjectResult;

        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void Get_ShouldReturnFileStreamResult_WhenFileExists()
    {
        var id = Guid.NewGuid();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("file data"));
        var mockStorage = new Mock<IFileStorage>();
        mockStorage.Setup(s => s.GetFile(id)).Returns((stream, "text/plain", "file.txt"));

        var controller = new FileController(mockStorage.Object);
        var result = controller.Get(id) as FileStreamResult;

        Assert.NotNull(result);
        Assert.Equal("file.txt", result.FileDownloadName);
    }

    [Fact]
    public void Get_ShouldReturnNotFound_WhenFileMissing()
    {
        var mockStorage = new Mock<IFileStorage>();
        mockStorage.Setup(s => s.GetFile(It.IsAny<Guid>())).Throws<FileNotFoundException>();

        var controller = new FileController(mockStorage.Object);
        var result = controller.Get(Guid.NewGuid()) as NotFoundObjectResult;

        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public void GetAllId_ShouldReturnIds()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var mockStorage = new Mock<IFileStorage>();
        mockStorage.Setup(s => s.GetAllIds()).Returns(ids);

        var controller = new FileController(mockStorage.Object);
        var result = controller.GetAllId() as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(ids, result.Value);
    }
}
