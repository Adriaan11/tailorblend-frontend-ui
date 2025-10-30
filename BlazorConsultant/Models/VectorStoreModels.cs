using System.Text.Json.Serialization;

namespace BlazorConsultant.Models;

/// <summary>
/// Vector Store metadata - represents an embedding dataset
/// </summary>
public record VectorStoreMetadata
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("vector_store_id")]
    public string? VectorStoreId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("source_file")]
    public string? SourceFile { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("item_count")]
    public int ItemCount { get; set; }
}

/// <summary>
/// Response from /api/vector-stores/list endpoint
/// </summary>
public record VectorStoreListResponse
{
    [JsonPropertyName("stores")]
    public List<VectorStoreMetadata>? Stores { get; set; }
}

/// <summary>
/// Response from /api/vector-stores/upload endpoint
/// </summary>
public record UploadVectorStoreResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("vector_store_id")]
    public string? VectorStoreId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("item_count")]
    public int ItemCount { get; set; }
}

/// <summary>
/// Response from /api/vector-stores/activate endpoint
/// </summary>
public record ActivateVectorStoreResponse
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("vector_store_id")]
    public string? VectorStoreId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
