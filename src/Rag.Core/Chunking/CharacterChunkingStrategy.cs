using Rag.Core.Models;
using Rag.Core.Extensions;

namespace Rag.Core.Chunking;

public class CharacterChunkingStrategy : IChunkingStrategy
{

    private readonly int _chunkSize;
    private readonly int _overlapSize;

    public CharacterChunkingStrategy(int chunkSize = 500, int overlapSize = 50)
    {
        if (chunkSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be greater than 0");
        }

        if (overlapSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(overlapSize), "Overlap size must not be a negative number.");
        }

        if (overlapSize >= chunkSize)
        {
            throw new ArgumentException("Overlap size must be less than Chunk Size", nameof(overlapSize));
        }

        this._chunkSize = chunkSize;
        this._overlapSize = overlapSize;
    }

    public IReadOnlyList<DocumentChunk> Chunk(Document document)
    {

        ArgumentNullException.ThrowIfNull(document);

        if (document.Content.IsNullOrWhitespace())
        {
            throw new ArgumentException("Null or empty document content detected.", nameof(document.Content));
        }


        var chunks = new List<DocumentChunk>();

        var stepSize = _chunkSize - _overlapSize;

        for (int start = 0, chunkIndex = 0;
             start < document.Content.Length;
             start += stepSize)
        {
            var remainingLength = document.Content.Length - start;

            var readLength = Math.Min(
                _chunkSize,
                remainingLength);

            var textChunk = document.Content.Substring(
                start,
                readLength);

            if (textChunk.IsNullOrWhitespace())
            {
                continue;
            }

            var chunk = new DocumentChunk(
                DocumentId: document.Id,
                ChunkId: $"{document.Id}:{chunkIndex}",
                Text: textChunk,
                ChunkIndex: chunkIndex);

            chunks.Add(chunk);
            chunkIndex++;

            if (start + readLength >= document.Content.Length)
            {
                break;
            }
        }


        return chunks.AsReadOnly();

    }
}