using CommunityToolkit.VectorData.SqliteVec;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

public class Hotel
{
    [VectorStoreKey]
    public int HotelId { get; set; }

    [VectorStoreData(StorageName = "hotel_name")]
    public string? HotelName { get; set; }

    [VectorStoreData(StorageName = "hotel_description")]
    public string? Description { get; set; }

    [VectorStoreVector(dimensions: 4096, DistanceFunction = DistanceFunction.CosineDistance)]
    public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }
}

public static class VectorDatabaseAgent
{
    private const string ConnectionString = "Data Source=vector.db";
    private const string CollectionName = "skhotels";

    [Description("将一段文本作为 chunk 写入 SQLite 向量数据库")]
    public static async Task<string> AddTextChunk(
        [Description("要写入的文本内容")] string text,
        [Description("可选的 chunk 编号，例如文件名-序号。不传会自动生成")] string? chunkId = null)
    {
        var embeddingGenerator = new OllamaEmbeddingGenerator(
            new Uri("http://localhost:11434"),
            "qwen3-embedding"
        );

        var vectorStore = new SqliteVectorStore(ConnectionString);
        var collection = vectorStore.GetCollection<int, Hotel>(CollectionName);
        await collection.EnsureCollectionExistsAsync();

        var embedding = (await embeddingGenerator.GenerateAsync(text)).Vector;

        int id = Math.Abs((chunkId ?? Guid.NewGuid().ToString()).GetHashCode());
        if (id == int.MinValue)
        {
            id = int.MaxValue;
        }

        await collection.UpsertAsync(new Hotel
        {
            HotelId = id,
            HotelName = chunkId ?? $"chunk_{id}",
            Description = text,
            DescriptionEmbedding = embedding
        });

        return $"已写入向量数据库，chunkId: {chunkId ?? id.ToString()}";
    }

    [Description("读取指定文件，将文件内容按字符数切分为多个 chunk 并写入 SQLite 向量数据库")]
    public static async Task<string> AddFileChunks(
        [Description("文件完整路径")] string filePath,
        [Description("每个 chunk 的最大字符数（默认500）")] int chunkSize = 500)
    {
        if (!File.Exists(filePath))
        {
            return $"文件不存在: {filePath}";
        }

        string content = await File.ReadAllTextAsync(filePath);
        var chunks = SplitText(content, chunkSize);
        if (chunks.Count == 0)
        {
            return "文件内容为空，没有可写入的 chunk";
        }

        string baseId = Path.GetFileNameWithoutExtension(filePath);
        for (int i = 0; i < chunks.Count; i++)
        {
            await AddTextChunk(chunks[i], $"{baseId}-{i + 1}");
        }

        return $"成功写入 {chunks.Count} 个 chunk";
    }

    private static List<string> SplitText(string text, int chunkSize)
    {
        var chunks = new List<string>();
        if (string.IsNullOrEmpty(text))
        {
            return chunks;
        }

        for (int i = 0; i < text.Length; i += chunkSize)
        {
            int length = Math.Min(chunkSize, text.Length - i);
            chunks.Add(text.Substring(i, length));
        }

        return chunks;
    }
}
