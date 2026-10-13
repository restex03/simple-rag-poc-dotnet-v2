using System.ComponentModel.DataAnnotations;
public class EmbeddingModelOptions
{
    public const string SectionName = "EmbeddingModel";
    [MinLength(1)]
    [Required]
    public string Model { get; set; } = default!;
}