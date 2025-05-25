using System.Text;
using System.Text.Json;
using FileAnalisysService.Storage;

namespace FileAnalisysService;


   public class WordCloudService: IWordCloudService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AnalysisDbContext _db;
    private readonly IWordCloudStorage _wordCloudStorage;
    

    public WordCloudService(AnalysisDbContext db, IHttpClientFactory httpClientFactory, IWordCloudStorage wordCloudStorage)
    {
        _httpClientFactory = httpClientFactory;
        _db = db;
        _wordCloudStorage = wordCloudStorage;
    }

    public void GenerateWordCloud(string text, Guid fileId)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text is empty");

        var requestBody = new
        {
            format = "png",
            width = 500,
            height = 500,
            fontScale = 15,
            scale = "linear",
            removeStopwords = true,
            text = text
        };

        string json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var client = _httpClientFactory.CreateClient();
        var response = client.PostAsync("https://quickchart.io/wordcloud", content).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();

        byte[] imageBytes = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();

        _wordCloudStorage.Save(imageBytes, fileId);
    }
    

    public (Stream stream, string contentType, string fileName) GetWordCloud(Guid fileId)
    {   
        Console.WriteLine("Getting word cloud");
        return _wordCloudStorage.Get(fileId);
    }


}

