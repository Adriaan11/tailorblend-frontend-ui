using System.Net.Http.Json;
using BlazorConsultant.Models;
using BlazorConsultant.Configuration;

namespace BlazorConsultant.Services;

/// <summary>
/// Vector Store Service - manages embedding datasets
/// Communicates with Python backend /api/vector-stores endpoints
/// Handles listing, uploading, activating, and deleting vector stores
/// </summary>
public class VectorStoreService : IVectorStoreService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISessionService _sessionService;
    private readonly ILogger<VectorStoreService> _logger;

    public VectorStoreService(
        IHttpClientFactory httpClientFactory,
        ISessionService sessionService,
        ILogger<VectorStoreService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _sessionService = sessionService;
        _logger = logger;
    }

    /// <summary>
    /// List all available vector stores from backend
    /// </summary>
    public async Task<List<VectorStoreMetadata>> ListVectorStoresAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📊 Fetching vector stores list...");

            var client = _httpClientFactory.CreateClient("PythonAPI");

            // Set timeout
            using var timeoutCts = TimeoutPolicy.CreateTimeoutTokenSource(
                TimeoutPolicy.HttpRequestTimeout,
                cancellationToken);

            var response = await client.GetAsync("/api/vector-stores", timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(timeoutCts.Token);
                _logger.LogError("❌ Failed to list vector stores: {StatusCode} - {Content}",
                    response.StatusCode, content);
                throw new HttpRequestException($"Failed to list vector stores: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<VectorStoreListResponse>(cancellationToken: timeoutCts.Token);

            _logger.LogInformation("✅ Retrieved {Count} vector stores", result?.Stores?.Count ?? 0);

            return result?.Stores ?? new List<VectorStoreMetadata>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ HTTP error listing vector stores");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Unexpected error listing vector stores");
            throw;
        }
    }

    /// <summary>
    /// Upload a JSON file and create a new vector store
    /// </summary>
    public async Task<VectorStoreMetadata> UploadVectorStoreAsync(
        Stream fileStream,
        string fileName,
        string datasetName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📤 Uploading vector store: {FileName} ({DatasetName})", fileName, datasetName);

            var client = _httpClientFactory.CreateClient("PythonAPI");

            // Set timeout for upload (longer for file processing)
            using var timeoutCts = TimeoutPolicy.CreateTimeoutTokenSource(
                TimeSpan.FromMinutes(5), // 5 minute timeout for upload
                cancellationToken);

            // Build multipart form data
            using var content = new MultipartFormDataContent();

            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            content.Add(fileContent, "file", fileName);
            content.Add(new StringContent(datasetName), "name");

            var response = await client.PostAsync("/api/vector-stores/upload", content, timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(timeoutCts.Token);
                _logger.LogError("❌ Failed to upload vector store: {StatusCode} - {Content}",
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"Failed to upload vector store: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<VectorStoreMetadata>(cancellationToken: timeoutCts.Token);

            _logger.LogInformation("✅ Vector store uploaded successfully: {Id} ({Name})", result?.Id, result?.Name);

            return result ?? throw new InvalidOperationException("No response from upload");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ HTTP error uploading vector store");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Unexpected error uploading vector store");
            throw;
        }
    }

    /// <summary>
    /// Activate a vector store for the current session
    /// </summary>
    public async Task<VectorStoreMetadata> ActivateVectorStoreAsync(
        string vectorStoreId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🎯 Activating vector store: {VectorStoreId}", vectorStoreId);

            var client = _httpClientFactory.CreateClient("PythonAPI");

            // Set timeout
            using var timeoutCts = TimeoutPolicy.CreateTimeoutTokenSource(
                TimeoutPolicy.HttpRequestTimeout,
                cancellationToken);

            var url = $"/api/vector-stores/activate?session_id={Uri.EscapeDataString(_sessionService.SessionId)}&vector_store_id={Uri.EscapeDataString(vectorStoreId)}";

            var response = await client.PostAsync(url, null, timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(timeoutCts.Token);
                _logger.LogError("❌ Failed to activate vector store: {StatusCode} - {Content}",
                    response.StatusCode, content);
                throw new HttpRequestException($"Failed to activate vector store: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<VectorStoreMetadata>(cancellationToken: timeoutCts.Token);

            _logger.LogInformation("✅ Vector store activated: {Name}", result?.Name);

            return result ?? throw new InvalidOperationException("No response from activate");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ HTTP error activating vector store");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Unexpected error activating vector store");
            throw;
        }
    }

    /// <summary>
    /// Get the currently active vector store for the session
    /// </summary>
    public async Task<VectorStoreMetadata> GetActiveVectorStoreAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📍 Fetching active vector store for session {SessionId}", _sessionService.SessionId);

            var client = _httpClientFactory.CreateClient("PythonAPI");

            // Set timeout
            using var timeoutCts = TimeoutPolicy.CreateTimeoutTokenSource(
                TimeoutPolicy.HttpRequestTimeout,
                cancellationToken);

            var url = $"/api/vector-stores/active?session_id={Uri.EscapeDataString(_sessionService.SessionId)}";

            var response = await client.GetAsync(url, timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(timeoutCts.Token);
                _logger.LogError("❌ Failed to get active vector store: {StatusCode} - {Content}",
                    response.StatusCode, content);
                throw new HttpRequestException($"Failed to get active vector store: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<VectorStoreMetadata>(cancellationToken: timeoutCts.Token);

            _logger.LogInformation("✅ Active vector store: {Name}", result?.Name);

            return result ?? throw new InvalidOperationException("No response from get active");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ HTTP error getting active vector store");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Unexpected error getting active vector store");
            throw;
        }
    }

    /// <summary>
    /// Upload multiple JSON files and create a single vector store using batch upload
    /// </summary>
    public async Task<VectorStoreMetadata> UploadMultipleFilesAsync(
        List<(Stream stream, string fileName)> files,
        string datasetName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📤 Uploading {Count} files for dataset: {DatasetName}",
                files.Count, datasetName);

            var client = _httpClientFactory.CreateClient("PythonAPI");

            // Set timeout for upload (longer for multiple files)
            using var timeoutCts = TimeoutPolicy.CreateTimeoutTokenSource(
                TimeSpan.FromMinutes(10), // 10 minute timeout for batch uploads
                cancellationToken);

            // Build multipart form data
            using var content = new MultipartFormDataContent();

            // Add all files with field name "files" (FastAPI expects List[UploadFile] with this name)
            foreach (var (stream, fileName) in files)
            {
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                content.Add(fileContent, "files", fileName);  // Note: "files" (plural) matches FastAPI parameter
            }

            // Add dataset name
            content.Add(new StringContent(datasetName), "name");

            var response = await client.PostAsync("/api/vector-stores/upload",
                content, timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(timeoutCts.Token);
                _logger.LogError("❌ Failed to upload vector store: {StatusCode} - {Content}",
                    response.StatusCode, errorContent);
                throw new HttpRequestException(
                    $"Failed to upload vector store: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<VectorStoreMetadata>(
                cancellationToken: timeoutCts.Token);

            _logger.LogInformation("✅ Vector store uploaded successfully: {Id} ({Name}) with {Count} files",
                result?.Id, result?.Name, files.Count);

            return result ?? throw new InvalidOperationException("No response from upload");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ HTTP error uploading vector store");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Unexpected error uploading vector store");
            throw;
        }
    }

    /// <summary>
    /// Delete a vector store
    /// </summary>
    public async Task DeleteVectorStoreAsync(string storeId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🗑️  Deleting vector store: {StoreId}", storeId);

            var client = _httpClientFactory.CreateClient("PythonAPI");

            // Set timeout
            using var timeoutCts = TimeoutPolicy.CreateTimeoutTokenSource(
                TimeoutPolicy.HttpRequestTimeout,
                cancellationToken);

            var response = await client.DeleteAsync($"/api/vector-stores/{Uri.EscapeDataString(storeId)}", timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(timeoutCts.Token);
                _logger.LogError("❌ Failed to delete vector store: {StatusCode} - {Content}",
                    response.StatusCode, content);
                throw new HttpRequestException($"Failed to delete vector store: {response.StatusCode}");
            }

            _logger.LogInformation("✅ Vector store deleted: {StoreId}", storeId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ HTTP error deleting vector store");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Unexpected error deleting vector store");
            throw;
        }
    }
}

/// <summary>
/// Vector Store Service Interface
/// </summary>
public interface IVectorStoreService
{
    Task<List<VectorStoreMetadata>> ListVectorStoresAsync(CancellationToken cancellationToken = default);
    Task<VectorStoreMetadata> UploadVectorStoreAsync(Stream fileStream, string fileName, string datasetName, CancellationToken cancellationToken = default);
    Task<VectorStoreMetadata> UploadMultipleFilesAsync(List<(Stream stream, string fileName)> files, string datasetName, CancellationToken cancellationToken = default);
    Task<VectorStoreMetadata> ActivateVectorStoreAsync(string vectorStoreId, CancellationToken cancellationToken = default);
    Task<VectorStoreMetadata> GetActiveVectorStoreAsync(CancellationToken cancellationToken = default);
    Task DeleteVectorStoreAsync(string storeId, CancellationToken cancellationToken = default);
}
