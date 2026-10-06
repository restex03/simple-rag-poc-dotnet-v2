using System.ComponentModel.DataAnnotations;

public sealed class QdrantOptions
{
    public const string SectionName = "Qdrant";

    [MinLength(1)]
    public string BaseAddress { get; init; } = "";

}