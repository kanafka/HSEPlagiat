namespace FileAnalisysService;

public interface IWordCloudService
{
    void GenerateWordCloud(string text, Guid fileId);
    (Stream stream, string contentType, string fileName) GetWordCloud(Guid fileId);
    
}