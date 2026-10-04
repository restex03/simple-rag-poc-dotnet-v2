using Rag.Core.Chunking;
using Rag.Core.Models;
namespace Rag.Core.Tests;

public class CharacterChunkingStrategy_Tests
{

    [Fact]
    public void CanInstantiate()
    {
        var sut = new CharacterChunkingStrategy();
    }

    [Fact]
    public void Ctor_Guards_NegativeChunkSize()
    {
        var result = Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterChunkingStrategy(-1));
        Assert.Contains("Chunk size must be greater than 0", result.Message);
    }

    [Fact]
    public void Ctor_Guards_ZeroChunkSize()
    {
        var result = Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterChunkingStrategy(0));
        Assert.Contains("Chunk size must be greater than 0", result.Message);
    }

    [Fact]
    public void Ctor_Guards_NegativeOverlapSize()
    {
        var result = Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterChunkingStrategy(1, -1));
        Assert.Contains("Overlap size must not be a negative number.", result.Message);
    }

    [Fact]
    public void Ctor_Guards_OverlapEqualToOrGreaterThanChunkSize()
    {
        var equalResult = Assert.Throws<ArgumentException>(() => new CharacterChunkingStrategy(10, 10));
        Assert.Equal("overlapSize", equalResult.ParamName);
        Assert.Contains("Overlap size must be less than Chunk Size", equalResult.Message);

        var greaterResult = Assert.Throws<ArgumentException>(() => new CharacterChunkingStrategy(10, 11));
        Assert.Equal("overlapSize", greaterResult.ParamName);
        Assert.Contains("Overlap size must be less than Chunk Size", greaterResult.Message);
    }

    [Fact]
    public void Chunk_Guards_NullDocument()
    {
        var sut = new CharacterChunkingStrategy();

        var result = Assert.Throws<ArgumentNullException>(() => sut.Chunk(null!));

        Assert.Equal("document", result.ParamName);
    }

    [Fact]
    public void Chunk_Guards_NullDocumentContent()
    {
        var sut = new CharacterChunkingStrategy();
        var document = new Document("id", "title", null!);

        var result = Assert.Throws<ArgumentException>(() => sut.Chunk(document));

        Assert.Equal("Content", result.ParamName);
        Assert.Contains("Null or empty document content detected.", result.Message);
    }

    [Fact]
    public void Chunk_Guards_EmptyDocumentContent()
    {
        var sut = new CharacterChunkingStrategy();

        var document = new Document("id", "title", "");
        var result = Assert.Throws<ArgumentException>(() => sut.Chunk(document));

        Assert.Equal("Content", result.ParamName);
        Assert.Contains("Null or empty document content detected.", result.Message);
    }


    [Fact]
    public void Chunk_Returns_CorrectNumberOfChunks()
    {
        var chunkSize = 100;
        var overlapSize = 10;
        var sut = new CharacterChunkingStrategy(chunkSize, overlapSize);
        var documentText = "Whales communicate primarily through acoustic signals and body language, utilizing the ocean's efficiency in conducting sound over vast distances." +
            "Wolves communicate via vocalizations, body language, scent marking, and tactile interactions.";
        var expectedNumChunks = (int)Math.Ceiling((double)documentText.Length / chunkSize);


        var doc = new Document(
            Id: Guid.NewGuid().ToString(),
            Title: "Communication within the animal kingdom",
            Content: documentText);

        var result = sut.Chunk(doc);
        Assert.Equal(expectedNumChunks, result.Count());
        Assert.All(result, x => Assert.Equal(doc.Id, x.DocumentId));

    }

    [Fact]
    public void Chunk_Returns_SingleChunkForContentSmallerThanChunkSize()
    {
        var chunkSize = 100;
        var overlapSize = 10;
        var sut = new CharacterChunkingStrategy(chunkSize, overlapSize);
        var documentText = "Whales are cool.";
        var documentId = Guid.NewGuid().ToString();
        var doc = new Document(
            Id: documentId,
            Title: "Communication within the animal kingdom",
            Content: documentText);

        var result = sut.Chunk(doc);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal($"{documentId}:0", result[0].ChunkId);
        Assert.Equal(0, result[0].ChunkIndex);
        Assert.Equal(documentText, result[0].Text);
    }


    [Fact]
    public void Chunk_Returns_OneChunkWhenDocumentFitsExactlyInOneChunk()
    {
        // Arrange
        var content = new string('a', 500);

        var document = new Document(
            Id: "doc-1",
            Title: "Test Document",
            Content: content);

        var sut = new CharacterChunkingStrategy(
            chunkSize: 500,
            overlapSize: 50);

        // Act
        var chunks = sut.Chunk(document);

        // Assert
        Assert.Single(chunks);
        Assert.Equal(content, chunks[0].Text);
    }


    [Fact]
    public void Chunk_OverlapsAdjacentChunksByConfiguredAmount()
    {
        var sut = new CharacterChunkingStrategy(
            chunkSize: 10,
            overlapSize: 3);

        var document = new Document(
            Id: "doc-1",
            Title: "Test",
            Content: "abcdefghijklmnopqrstuvwxyz");

        var chunks = sut.Chunk(document);

        Assert.Equal("abcdefghij", chunks[0].Text);
        Assert.Equal("hijklmnopq", chunks[1].Text);

        // Last 3 chars of chunk 0 == first 3 chars of chunk 1
        Assert.Equal(
            chunks[0].Text[^3..],
            chunks[1].Text[..3]);
    }

    [Fact]
    public void Chunk_UsesStepSizeToDetermineNextChunkStart()
    {
        var sut = new CharacterChunkingStrategy(
            chunkSize: 5,
            overlapSize: 2);

        var document = new Document(
            Id: "doc-1",
            Title: "Test",
            Content: "0123456789");

        var chunks = sut.Chunk(document);

        Assert.Equal(3, chunks.Count);

        Assert.Equal("01234", chunks[0].Text);
        Assert.Equal("34567", chunks[1].Text);
        Assert.Equal("6789", chunks[2].Text);
    }

    [Fact]
    public void Chunk_FinalChunkMayBeSmallerThanChunkSize()
    {
        var sut = new CharacterChunkingStrategy(
            chunkSize: 5,
            overlapSize: 1);

        var document = new Document(
            Id: "doc-1",
            Title: "Test",
            Content: "abcdefgh");

        var chunks = sut.Chunk(document);

        Assert.Equal(2, chunks.Count);

        Assert.Equal("abcde", chunks[0].Text);
        Assert.Equal("efgh", chunks[1].Text);
    }

    [Fact]
    public void Chunk_DoesNotGenerateRedundantTrailingChunk()
    {
        var sut = new CharacterChunkingStrategy(
            chunkSize: 10,
            overlapSize: 3);

        var document = new Document(
            Id: "doc-1",
            Title: "Test",
            Content: new string('a', 17));

        var chunks = sut.Chunk(document);

        Assert.Equal(2, chunks.Count);
        Assert.Equal(10, chunks[0].Text.Length);
        Assert.Equal(10, chunks[1].Text.Length);
    }

    [Fact]
    public void Chunk_WithZeroOverlap_ProducesNonOverlappingChunks()
    {
        var sut = new CharacterChunkingStrategy(
            chunkSize: 5,
            overlapSize: 0);

        var document = new Document(
            Id: "doc-1",
            Title: "Test",
            Content: "abcdefghij");

        var chunks = sut.Chunk(document);

        Assert.Equal(2, chunks.Count);
        Assert.Equal("abcde", chunks[0].Text);
        Assert.Equal("fghij", chunks[1].Text);
    }

    [Fact]
    public void Chunk_WithOverlapOneLessThanChunkSize_AdvancesOneCharacterAtATime()
    {
        var sut = new CharacterChunkingStrategy(
            chunkSize: 4,
            overlapSize: 3);

        var document = new Document(
            Id: "doc-1",
            Title: "Test",
            Content: "abcdef");

        var chunks = sut.Chunk(document);

        Assert.Equal(3, chunks.Count);

        Assert.Equal("abcd", chunks[0].Text);
        Assert.Equal("bcde", chunks[1].Text);
        Assert.Equal("cdef", chunks[2].Text);
    }

    [Fact]
    public void Chunk_AssignsSequentialChunkIdsAndIndexes()
    {
        var sut = new CharacterChunkingStrategy(
            chunkSize: 5,
            overlapSize: 2);

        var document = new Document(
            Id: "doc-123",
            Title: "Test",
            Content: "0123456789");

        var chunks = sut.Chunk(document);

        Assert.Equal("doc-123:0", chunks[0].ChunkId);
        Assert.Equal(0, chunks[0].ChunkIndex);

        Assert.Equal("doc-123:1", chunks[1].ChunkId);
        Assert.Equal(1, chunks[1].ChunkIndex);

        Assert.Equal("doc-123:2", chunks[2].ChunkId);
        Assert.Equal(2, chunks[2].ChunkIndex);

        Assert.All(
            chunks,
            chunk => Assert.Equal("doc-123", chunk.DocumentId));
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("     ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void Chunk_Guards_WhitespaceOnlyDocumentContent(string content)
    {
        var sut = new CharacterChunkingStrategy();

        var document = new Document(
            "id",
            "title",
            content);

        var result = Assert.Throws<ArgumentException>(
            () => sut.Chunk(document));

        Assert.Equal("Content", result.ParamName);
    }
}
