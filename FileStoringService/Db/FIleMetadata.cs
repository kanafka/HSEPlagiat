namespace File_Storing_Service;

using System.ComponentModel.DataAnnotations;

public class FileMetadata
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Hash { get; set; } = string.Empty;

    [Required]
    public string Location { get; set; } = string.Empty;
}
