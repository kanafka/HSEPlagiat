using Xunit;
using Moq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ApiGateway;
using Moq.Protected;
using System.Text;
using System.IO;

public class GatewayControllerTests
{
    private GatewayController CreateController(HttpResponseMessage fakeResponse)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(fakeResponse);

        var httpClient = new HttpClient(handlerMock.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        return new GatewayController(factoryMock.Object);
    }

    [Fact]
    public void UploadFile_ShouldReturnJsonContent()
    {
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"id\":\"123\", \"message\":\"Файл успешно сохранён\"}", Encoding.UTF8, "application/json")
        };

        var controller = CreateController(response);

        var fileBytes = Encoding.UTF8.GetBytes("hello");
        var formFile = new FormFile(new MemoryStream(fileBytes), 0, fileBytes.Length, "file", "file.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        var result = controller.UploadFile(formFile) as ContentResult;

        Assert.NotNull(result);
        Assert.Equal("application/json", result.ContentType);
        Assert.Contains("Файл успешно", result.Content);
    }

    [Fact]
    public void GetFile_ShouldReturnFile_WhenExists()
    {
        var fileBytes = Encoding.UTF8.GetBytes("test content");
        var stream = new MemoryStream(fileBytes);
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StreamContent(stream)
        };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileName = "\"testfile.txt\""
        };

        var controller = CreateController(response);

        var result = controller.GetFile(Guid.NewGuid()) as FileStreamResult;

        Assert.NotNull(result);
        Assert.Equal("application/octet-stream", result.ContentType);
        Assert.Equal("testfile.txt", result.FileDownloadName);
    }

    [Fact]
    public void GetFile_ShouldReturnError_WhenFileMissing()
    {
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound,
            Content = new StringContent("Not Found")
        };

        var controller = CreateController(response);
        var result = controller.GetFile(Guid.NewGuid()) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public void GetWorldCloud_ShouldReturnFile()
    {
        var fileBytes = Encoding.UTF8.GetBytes("cloud image");
        var stream = new MemoryStream(fileBytes);
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StreamContent(stream)
        };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileName = "\"cloud.png\""
        };

        var controller = CreateController(response);
        var result = controller.GetWorldCloud(Guid.NewGuid()) as FileStreamResult;

        Assert.NotNull(result);
        Assert.Equal("image/png", result.ContentType);
        Assert.Equal("cloud.png", result.FileDownloadName);
    }

    [Fact]
    public void Analyze_ShouldReturnJson()
    {
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"similarity\":0.85}", Encoding.UTF8, "application/json")
        };

        var controller = CreateController(response);
        var result = controller.Analyze(Guid.NewGuid()) as ContentResult;

        Assert.NotNull(result);
        Assert.Equal("application/json", result.ContentType);
        Assert.Contains("similarity", result.Content);
    }

    [Fact]
    public void WordAnalyze_ShouldReturnJson()
    {
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"wordCount\":123}", Encoding.UTF8, "application/json")
        };

        var controller = CreateController(response);
        var result = controller.WordAnalyze(Guid.NewGuid()) as ContentResult;

        Assert.NotNull(result);
        Assert.Equal("application/json", result.ContentType);
        Assert.Contains("wordCount", result.Content);
    }
}
