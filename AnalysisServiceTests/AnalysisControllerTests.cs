using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using FileAnalisysService;

public class AnalysisControllerTests
{
    [Fact]
    public void GetWordCloud_ShouldReturnFileResult()
    {
        // Arrange
        var analysisServiceMock = new Mock<IAnalysisService>();
        var wordCloudServiceMock = new Mock<IWordCloudService>();
        var fileId = Guid.NewGuid();
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        wordCloudServiceMock.Setup(s => s.GetWordCloud(fileId)).Returns((stream, "image/png", "wordcloud.png"));

        var controller = new AnalysisController(analysisServiceMock.Object, wordCloudServiceMock.Object);

        // Act
        var result = controller.GetWordCloud(fileId);

        // Assert
        var fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("image/png", fileResult.ContentType);
        Assert.Equal("wordcloud.png", fileResult.FileDownloadName);
    }

    [Fact]
    public void Analyze_ShouldReturnOkResult()
    {
        // Arrange
        var analysisServiceMock = new Mock<IAnalysisService>();
        var wordCloudServiceMock = new Mock<IWordCloudService>();
        var fileId = Guid.NewGuid();
        var analysisResult = new AnalysisResult { FileId = fileId, Similarities = 0.8 };
        analysisServiceMock.Setup(s => s.PlagiatAnalyze(fileId)).Returns(analysisResult);

        var controller = new AnalysisController(analysisServiceMock.Object, wordCloudServiceMock.Object);

        // Act
        var result = controller.Analyze(fileId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(analysisResult, okResult.Value);
    }

    [Fact]
    public void WordAnalyze_ShouldReturnOkResult()
    {
        // Arrange
        var analysisServiceMock = new Mock<IAnalysisService>();
        var wordCloudServiceMock = new Mock<IWordCloudService>();
        var fileId = Guid.NewGuid();
        var wordAnalysisResult = new WordAnalysisResult { WordCount = 10, ParagraphCount = 2, CharacterCount = 50 };
        analysisServiceMock.Setup(s => s.WordAnalyze(fileId)).Returns(wordAnalysisResult);

        var controller = new AnalysisController(analysisServiceMock.Object, wordCloudServiceMock.Object);

        // Act
        var result = controller.WordAnalyze(fileId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(wordAnalysisResult, okResult.Value);
    }
}
