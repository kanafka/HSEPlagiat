using System.Text.Json;
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
        var normalizedText = NormalizeText(text); // Нормализуем текст (удаление лишних пробелов и т.д.)

        // Сохраняем, если ещё нет
        if (_db.Files.Any(f => f.FileId == fileId))
        {
            Console.WriteLine("gghhhjj123 euzhe st");
            return new AnalysisResult()
                { FileId = fileId, Similarities = _db.Files.Single(f => f.FileId == fileId).Similarities };
        }

        // Сравниваем с другими
        var similarities = new List<FileSimilarity>();
        
        
        var responseIds = client.GetAsync("http://file-storage:8001/file/getAllId").Result;
        var json = responseIds.Content.ReadAsStringAsync().Result;
        var ids = JsonSerializer.Deserialize<List<Guid>>(json);
        foreach (var other in ids)
        {
            if (other == fileId)
            {
                continue;
            }
            var otherText = client.GetAsync($"http://file-storage:8001/file/get/{other}").Result.Content.ReadAsStringAsync().Result;
            var normalizedOtherText = NormalizeText(otherText); // Нормализуем текст другого файла
            
            int commonCharCount = CountCommonCharacters(normalizedText, normalizedOtherText);
            int minChars = Math.Min(normalizedText.Length, normalizedOtherText.Length);
            int maxChars = Math.Max(normalizedText.Length, normalizedOtherText.Length);

            // Вычисляем схожесть как минимальное количество символов / максимальное
            double similarity = (double)commonCharCount / maxChars;
            if (similarity == 1)
            {
                Console.WriteLine(other);
            }

            similarities.Add(new FileSimilarity { ComparedTo = other, SimilarityPercentage = similarity });
            Console.WriteLine("ASD");
        }
        
        _db.Files.Add(new AnalyzedFile { FileId = fileId, Similarities = similarities.OrderByDescending(s => s.SimilarityPercentage).ToList()[0].SimilarityPercentage});
        _db.SaveChanges();
        Console.WriteLine(similarities.Count);
        return new AnalysisResult
        {
            FileId = fileId,
            Similarities = similarities.OrderByDescending(s => s.SimilarityPercentage).ToList()[0].SimilarityPercentage
        };
    }

    private int CountCommonCharacters(string text1, string text2)
    {
        // Считаем количество общих символов между двумя строками
        int commonCharCount = 0;

        // Преобразуем в массивы символов для быстрого подсчета
        var chars1 = text1.ToCharArray();
        var chars2 = text2.ToCharArray();

        // Создадим словарь для подсчета символов в первом тексте
        var charCount1 = chars1.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());

        // Считаем сколько символов из второго текста есть в первом
        foreach (var c in chars2)
        {
            if (charCount1.ContainsKey(c) && charCount1[c] > 0)
            {
                commonCharCount++;
                charCount1[c]--; // Уменьшаем количество оставшихся символов для текущего символа
            }
        }

        return commonCharCount;
    }
        private string NormalizeText(string input)
        {
            input = input.ToLowerInvariant();
            input = Regex.Replace(input, "[^a-zа-я0-9 ]", "");
            return input;
        }
    }