using System.ComponentModel.DataAnnotations;

public sealed class QdrantOptions
{
    public const string SectionName = "Qdrant";

    [MinLength(1)]
    public string Host { get; init; } = "";

    [Required]
    public int GrpcPort { get; init; }

    [Required]
    public int HttpPort { get; init; }

}