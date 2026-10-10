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
        var chunks = new List<DocumentChunk>();

        ArgumentNullException.ThrowIfNull(document);

        if (document.Content.IsNullOrWhitespace())
        {
            throw new ArgumentException("Null or empty document content detected.", nameof(document.Content));
        }



        var stepSize = _chunkSize - _overlapSize;
        var currPosition = 0;
        var chunkIndex = 0;
        while (true)
        {
            var readLength = (currPosition + _chunkSize) < document.Content.Length
                ? _chunkSize
                : document.Content.Length - currPosition;

            var rawText = document.Content.Substring(currPosition, readLength);
            var chunk = new DocumentChunk(document.Id, $"{document.Id}:{chunkIndex}", rawText, chunkIndex);
            chunks.Add(chunk);

            if ((currPosition + readLength) >= document.Content.Length)
            {
                break;
            }

            currPosition += stepSize;
            chunkIndex++;
        }

        return chunks.AsReadOnly();

    }
}