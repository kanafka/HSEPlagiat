using Xunit;
using Moq;
using Moq.Protected;
using System.Net.Http;
using System.Net;
using FileAnalisysService;
using FileAnalisysService.Storage;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class WordCloudServiceTests
{
    [Fact]
    public void GenerateWordCloud_ShouldCallExternalServiceAndStoreFile()
    {
        // Arrange
        var db = new AnalysisDbContext(new DbContextOptionsBuilder<AnalysisDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        var fileId = Guid.NewGuid();
        var byteResult = new byte[] { 1, 2, 3 };

        var handler = new Mock<HttpMessageHandler>();

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(byteResult)
            });

        var httpClient = new HttpClient(handler.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var storageMock = new Mock<IWordCloudStorage>();
        storageMock.Setup(s => s.Save(It.IsAny<byte[]>(), It.IsAny<Guid>()));

        var service = new WordCloudService(db, factoryMock.Object, storageMock.Object);

        // Act
        service.GenerateWordCloud("some meaningful text here", fileId);

        // Assert
        storageMock.Verify(
            s => s.Save(It.Is<byte[]>(b => b.SequenceEqual(byteResult)), fileId),
            Times.Once
        );
    }
}