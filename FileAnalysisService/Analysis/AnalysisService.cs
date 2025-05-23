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

        public WordAnalysisResult WordAnalysis(Guid fileId)
        {
            
            if (_db.Files.Any(f => f.FileId == fileId && f.WordAnalysis != null))
            {
                Console.WriteLine("gghhhjj123 euzhe st");
                return new WordAnalysisResult()
                {
                    WordCount = _db.Files.Single(f => f.FileId == fileId).WordAnalysis.WordCount,
                    ParagraphCount = _db.Files.Single(f => f.FileId == fileId).WordAnalysis.ParagraphCount,
                    CharacterCount = _db.Files.Single(f => f.FileId == fileId).WordAnalysis.CharacterCount
                };
            }
            
            var client = _httpClientFactory.CreateClient();
            string text = GetFile(fileId, client);
            
            var wordCount = Regex.Matches(text, @"\b\w+\b").Count;
            var letterCount = text.Count(char.IsLetter);
            var paragraphCount = text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries).Length;

            var file = _db.Files.SingleOrDefault(f => f.FileId == fileId);
            if (file != null)
            {
                // Initialize WordAnalysis if it's null
                if (file.WordAnalysis == null)
                {
                    file.WordAnalysis = new WordAnalysisResult(); // Ensure this matches your entity type
                }
        
                // Update values
                file.WordAnalysis.WordCount = wordCount;
                file.WordAnalysis.ParagraphCount = paragraphCount;
                file.WordAnalysis.CharacterCount = letterCount;
        
                _db.SaveChanges(); // Persist changes to the database
            }
    
            return new WordAnalysisResult()
            {
                WordCount = wordCount,
                ParagraphCount = paragraphCount,
                CharacterCount = letterCount
            };
        }


        private string GetFile(Guid fileId, HttpClient client)
        {
            var response = client.GetAsync($"http://file-storage:8001/file/get/{fileId}").Result;

            if (!response.IsSuccessStatusCode)
                throw new Exception("Не удалось получить файл");

            return response.Content.ReadAsStringAsync().Result;
        }
        public AnalysisResult PlagiatAnalyze(Guid fileId)
        {

            // Сохраняем, если ещё нет
            if (_db.Files.Any(f => f.FileId == fileId && f.Similarities != null))
            {
                Console.WriteLine("gghhhjj123 euzhe st");
                return new AnalysisResult()
                    { FileId = fileId, Similarities = _db.Files.Single(f => f.FileId == fileId).Similarities };
            }
            var client = _httpClientFactory.CreateClient(); 
            string text = GetFile(fileId, client);
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
                var otherText = GetFile(other, client);

                
                int commonCharCount = CountCommonCharacters(text, otherText);
                int maxChars = Math.Max(text.Length, otherText.Length);

                // Вычисляем схожесть как общее / максимальное
                double similarity = (double)commonCharCount / maxChars;
                if (similarity == 1)
                {
                    Console.WriteLine(other);
                }

                similarities.Add(new FileSimilarity { ComparedTo = other, SimilarityPercentage = similarity });
                Console.WriteLine("ASD");
            }
            
            _db.Files.Add(new AnalyzedFile { FileId = fileId, Similarities = similarities.OrderByDescending(s => s.SimilarityPercentage).ToList()[0].SimilarityPercentage, WordAnalysis = null});
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
    }