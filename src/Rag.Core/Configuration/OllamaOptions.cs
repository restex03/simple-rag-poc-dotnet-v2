using System.ComponentModel.DataAnnotations;

public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";

    [MinLength(1)]
    public string BaseAddress { get; init; } = "";

}