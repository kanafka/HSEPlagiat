using System.ComponentModel.DataAnnotations;

namespace FileAnalisysService;

public class WordCloudImage
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Location { get; set; } = string.Empty;
}