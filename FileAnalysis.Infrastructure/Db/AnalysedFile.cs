using System.ComponentModel.DataAnnotations;

namespace FileAnalisysService;

public class AnalyzedFile
{
    [Key]
    public Guid FileId { get; set; }
    public double? Similarities { get; set; }
    public WordAnalysisResult? WordAnalysis { get; set; }
}