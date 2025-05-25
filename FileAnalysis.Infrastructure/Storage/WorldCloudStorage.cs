namespace FileAnalisysService.Storage;

public class WordCloudFileStorage : IWordCloudStorage
{
    private readonly string _basePath;
    private readonly AnalysisDbContext _db;

    public WordCloudFileStorage(AnalysisDbContext db)
    {
        _db = db;
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "WordCloudImages");
        Directory.CreateDirectory(_basePath);
    }

    public void Save(byte[] imageBytes, Guid fileId)
    {
        var id = fileId;
        var fileName = $"{id}.png";
        var fullPath = Path.Combine(_basePath, fileName);

        File.WriteAllBytes(fullPath, imageBytes);

        var entity = new WordCloudImage
        {
            Id = id,
            Location = fullPath,

        };

        _db.WordCloudImages.Add(entity);
        _db.SaveChanges();

    }

    public (Stream stream, string contentType, string fileName) Get(Guid id)
    {
        var file = _db.WordCloudImages.Find(id);
        if (file == null || !System.IO.File.Exists(file.Location))
        {
            Console.WriteLine("Storage slomalsya");
            throw new FileNotFoundException();
        }
        Console.WriteLine("Storage nachal iskat");
        var stream = new FileStream(file.Location, FileMode.Open, FileAccess.Read);
        var contentType = "application/octet-stream";
        return (stream, contentType, "image.png");
    }
}
