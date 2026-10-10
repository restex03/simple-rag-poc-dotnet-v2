using System.ComponentModel.DataAnnotations;
public class ChatClientOptions
{
    public const string SectionName = "ChatClient";
    [MinLength(1)]
    [Required]
    public string BaseAddress { get; set; } = default!;

    [MinLength(1)]
    [Required]
    public string ApiKey { get; set; } = default!;

    [MinLength(1)]
    [Required]
    public string Model { get; set; } = default!;
}