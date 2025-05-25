using Xunit;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.EntityFrameworkCore;
using FileAnalisysService;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Moq.Protected;
public class SimpleAnalysisServiceTests
{
    private AnalysisDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AnalysisDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AnalysisDbContext(options);
    }

    [Fact]
    public void WordAnalyze_ShouldReturnCorrectCounts()
    {
        // Arrange
        var db = CreateDbContext();
        var fileId = Guid.NewGuid();
        db.Files.Add(new AnalyzedFile { FileId = fileId, WordAnalysis = new WordAnalysisResult { WordCount = -1 } });
        db.SaveChanges();

        var handler = new Mock<HttpMessageHandler>();
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Hello world.\n\nThis is a test.")
            });
        var httpClient = new HttpClient(handler.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var wordCloudMock = new Mock<IWordCloudService>();

        var service = new SimpleAnalysisService(db, factoryMock.Object, wordCloudMock.Object);

        // Act
        var result = service.WordAnalyze(fileId);

        // Assert
        Assert.Equal(6, result.WordCount);
        Assert.Equal(2, result.ParagraphCount);
        Assert.Equal(21, result.CharacterCount);
    }

    [Fact]
    public void PlagiatAnalyze_ShouldReturnCorrectSimilarity()
    {
        // Arrange
        var db = CreateDbContext();
        var fileId = Guid.NewGuid();
        var otherId = Guid.NewGuid();

        db.Files.Add(new AnalyzedFile { FileId = fileId, WordAnalysis = new WordAnalysisResult { WordCount = -1 } });
        db.Files.Add(new AnalyzedFile { FileId = otherId, WordAnalysis = new WordAnalysisResult { WordCount = -1 } });
        db.SaveChanges();

        var factoryMock = new Mock<IHttpClientFactory>();

        var client = new HttpClient(new MockHttpMessageHandler(fileId, otherId));
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

        var wordCloudMock = new Mock<IWordCloudService>();

        var service = new SimpleAnalysisService(db, factoryMock.Object, wordCloudMock.Object);

        // Act
        var result = service.PlagiatAnalyze(fileId);

        // Assert
        Assert.True(result.Similarities == null);
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Guid _fileId;
        private readonly Guid _otherId;

        public MockHttpMessageHandler(Guid fileId, Guid otherId)
        {
            _fileId = fileId;
            _otherId = otherId;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uri = request.RequestUri!.ToString();

            if (uri.Contains("/getAllId"))
            {
                var json = JsonSerializer.Serialize(new List<Guid> { _fileId, _otherId });
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Sample text file")
            });
        }
    }
}
