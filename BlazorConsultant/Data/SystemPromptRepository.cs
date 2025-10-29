using BlazorConsultant.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BlazorConsultant.Data;

/// <summary>
/// Dapper-based repository for SystemPrompt data access.
/// Provides direct SQL queries for CRUD operations.
/// </summary>
public class SystemPromptRepository : ISystemPromptRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SystemPromptRepository> _logger;

    public SystemPromptRepository(IConfiguration configuration, ILogger<SystemPromptRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration");
        _logger = logger;
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<IEnumerable<SystemPromptDto>> GetAllAsync(bool activeOnly = false)
    {
        using var connection = CreateConnection();
        var sql = activeOnly
            ? "SELECT * FROM SystemPrompts WHERE IsActive = 1 ORDER BY IsDefault DESC, CreatedAt DESC"
            : "SELECT * FROM SystemPrompts ORDER BY IsDefault DESC, CreatedAt DESC";

        try
        {
            return await connection.QueryAsync<SystemPromptDto>(sql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get prompts (activeOnly: {ActiveOnly})", activeOnly);
            throw;
        }
    }

    public async Task<SystemPromptDto?> GetByIdAsync(int id)
    {
        using var connection = CreateConnection();
        var sql = "SELECT * FROM SystemPrompts WHERE Id = @Id";

        try
        {
            return await connection.QuerySingleOrDefaultAsync<SystemPromptDto>(sql, new { Id = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get prompt by ID: {PromptId}", id);
            throw;
        }
    }

    public async Task<SystemPromptDto?> GetDefaultAsync()
    {
        using var connection = CreateConnection();
        var sql = "SELECT TOP 1 * FROM SystemPrompts WHERE IsDefault = 1 AND IsActive = 1";

        try
        {
            return await connection.QuerySingleOrDefaultAsync<SystemPromptDto>(sql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get default prompt");
            throw;
        }
    }

    public async Task<IEnumerable<SystemPromptDto>> SearchByNameAsync(string searchTerm)
    {
        using var connection = CreateConnection();
        var sql = "SELECT * FROM SystemPrompts WHERE Name LIKE @SearchTerm ORDER BY Name";

        try
        {
            return await connection.QueryAsync<SystemPromptDto>(sql, new { SearchTerm = $"%{searchTerm}%" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search prompts with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<int> CreateAsync(SystemPromptDto prompt)
    {
        using var connection = CreateConnection();
        var sql = @"
            INSERT INTO SystemPrompts (Name, Description, Content, IsActive, IsDefault, CreatedAt, UpdatedAt)
            VALUES (@Name, @Description, @Content, @IsActive, @IsDefault, GETUTCDATE(), GETUTCDATE());
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        try
        {
            var id = await connection.ExecuteScalarAsync<int>(sql, prompt);
            _logger.LogInformation("Created prompt {PromptId}: {PromptName}", id, prompt.Name);
            return id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create prompt: {PromptName}", prompt.Name);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(SystemPromptDto prompt)
    {
        using var connection = CreateConnection();
        var sql = @"
            UPDATE SystemPrompts
            SET Name = @Name,
                Description = @Description,
                Content = @Content,
                IsActive = @IsActive,
                IsDefault = @IsDefault,
                UpdatedAt = GETUTCDATE()
            WHERE Id = @Id";

        try
        {
            var rowsAffected = await connection.ExecuteAsync(sql, prompt);
            if (rowsAffected > 0)
            {
                _logger.LogInformation("Updated prompt {PromptId}: {PromptName}", prompt.Id, prompt.Name);
            }
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update prompt {PromptId}", prompt.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = CreateConnection();
        var sql = "DELETE FROM SystemPrompts WHERE Id = @Id";

        try
        {
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            if (rowsAffected > 0)
            {
                _logger.LogInformation("Deleted prompt {PromptId}", id);
            }
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete prompt {PromptId}", id);
            throw;
        }
    }

    public async Task<bool> SetDefaultAsync(int id)
    {
        using var connection = (SqlConnection)CreateConnection();
        await connection.OpenAsync();

        try
        {
            // Use transaction to ensure atomicity
            using var transaction = connection.BeginTransaction();

            try
            {
                // Clear all defaults
                await connection.ExecuteAsync(
                    "UPDATE SystemPrompts SET IsDefault = 0 WHERE IsDefault = 1",
                    transaction: transaction);

                // Set new default
                var rowsAffected = await connection.ExecuteAsync(
                    "UPDATE SystemPrompts SET IsDefault = 1, UpdatedAt = GETUTCDATE() WHERE Id = @Id",
                    new { Id = id },
                    transaction: transaction);

                transaction.Commit();

                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Set prompt {PromptId} as default", id);
                }
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set default prompt {PromptId}", id);
            throw;
        }
    }
}
