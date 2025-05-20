using System.ComponentModel.DataAnnotations;

namespace FileAnalisysService;

public class AnalyzedFile
{
    [Key]
    public Guid FileId { get; set; }

    [Required]
    public double Similarities { get; set; }
    
    [Required]
    public WordAnalysisResult WordAnalysis { get; set; }
}