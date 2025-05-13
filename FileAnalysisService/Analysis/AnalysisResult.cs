namespace FileAnalisysService;

public class FileSimilarity
{
    public Guid ComparedTo { get; set; }
    public double SimilarityPercentage { get; set; }
}

public class AnalysisResult
{
    public Guid FileId { get; set; }
    public List<FileSimilarity> Similarities { get; set; } = new();
}