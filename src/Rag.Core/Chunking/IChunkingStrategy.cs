using Rag.Core.Models;

namespace Rag.Core.Chunking;

/// <summary>
/// Defines a strategy for chunking documents into smaller pieces.
/// Note: The right strategy depends heavily on the corpus. 
/// API documentation, legal documents, earnings reports, source code, 
/// tables, FAQs, and conversational transcripts all have different natural boundaries.
/// The key is to choose a chunking strategy that respects the natural boundaries of the specific type of content you are working with.
/// </summary>
public interface IChunkingStrategy
{
    public IReadOnlyList<DocumentChunk> Chunk(Document document);
}