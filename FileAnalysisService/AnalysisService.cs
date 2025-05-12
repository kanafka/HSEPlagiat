using System.Text.RegularExpressions;

namespace FileAnalisysService;

public class SimpleAnalysisService : IAnalysisService
    {
        private readonly AnalysisDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;

        public SimpleAnalysisService(AnalysisDbContext db, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _httpClientFactory = httpClientFactory;
        }

        public AnalysisResult Analyze(Guid fileId)
        {
            var client = _httpClientFactory.CreateClient(); 
            var response = client.GetAsync($"http://file-storage:8001/file/get/{fileId}").Result;
            

            if (!response.IsSuccessStatusCode)
                throw new Exception("Не удалось получить файл");

            var text = response.Content.ReadAsStringAsync().Result;
            var normalized = NormalizeText(text);

            // Сохраняем, если ещё нет
            if (!_db.Files.Any(f => f.FileId == fileId))
            {
                _db.Files.Add(new AnalyzedFile { FileId = fileId, Content = normalized });
                _db.SaveChanges();
            }

            // Сравниваем с другими
            var similarities = new List<FileSimilarity>();

            foreach (var other in _db.Files.Where(f => f.FileId != fileId))
            {
                double similarity = ComputeSimilarity(normalized, other.Content);
                similarities.Add(new FileSimilarity { ComparedTo = other.FileId, SimilarityPercentage = similarity });
            }

            return new AnalysisResult
            {
                FileId = fileId,
                Similarities = similarities.OrderByDescending(s => s.SimilarityPercentage).ToList()
            };
        }

        private string NormalizeText(string input)
        {
            input = input.ToLowerInvariant();
            input = Regex.Replace(input, "[^a-zа-я0-9 ]", "");
            return input;
        }

        private double ComputeSimilarity(string text1, string text2)
        {
            var words1 = text1.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            var words2 = text2.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

            var intersection = words1.Intersect(words2).Count();
            var union = words1.Union(words2).Count();

            return union == 0 ? 0.0 : (double)intersection / union * 100.0;
        }
    }