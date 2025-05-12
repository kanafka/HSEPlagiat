using System.ComponentModel.DataAnnotations;

namespace FileAnalisysService;

public class AnalyzedFile
{
    [Key]
    public Guid FileId { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;
}