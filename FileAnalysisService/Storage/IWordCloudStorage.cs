namespace FileAnalisysService.Storage;

public interface IWordCloudStorage
{
    void Save(byte[] imageBytes, Guid fileId);
    (Stream stream, string contentType, string fileName) Get(Guid id);
}