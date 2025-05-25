namespace FileAnalisysService;

public class FileSimilarity
{
    public Guid ComparedTo { get; set; }
    public double SimilarityPercentage { get; set; }
}

public class WordAnalysisResult
{
    public int WordCount { get; set; }
    public int ParagraphCount { get; set; }
    public int CharacterCount { get; set; }
}

public class AnalysisResult
{
    public Guid FileId { get; set; }
    public double? Similarities { get; set; }
}