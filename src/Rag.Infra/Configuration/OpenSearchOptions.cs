using System.ComponentModel.DataAnnotations;
public class OpenSearchOptions
{
    public const string SectionName = "OpenSearch";
    [MinLength(1)]
    [Required]
    public string BaseAddress { get; set; } = default!;
}