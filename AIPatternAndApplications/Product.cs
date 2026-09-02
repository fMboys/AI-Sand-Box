using Microsoft.Extensions.VectorData;

public class Product
{
    [VectorStoreKey]
    public int Id { get; set; }
    [VectorStoreData]
    public string Name { get; set; }
    [VectorStoreData]
    public string Description { get; set; }
    [VectorStoreVector(1536, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Embedding { get; set; }
}