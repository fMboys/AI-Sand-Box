using Microsoft.Extensions.VectorData;

public class Document
{
    [VectorStoreKey]
    public string Id { get; set; } = string.Empty;
    [VectorStoreData]
    public string Title { get; set; } = string.Empty;
    [VectorStoreData]
    public string Content { get; set; } = string.Empty;
    [VectorStoreData]
    public string Category { get; set; } = string.Empty;
    [VectorStoreVector(1536, DistanceFunction = DistanceFunction.CosineDistance)]
    public ReadOnlyMemory<float> Embedding { get; set; }
}