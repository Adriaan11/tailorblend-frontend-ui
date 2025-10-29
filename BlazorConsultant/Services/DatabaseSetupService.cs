using Dapper;
using Microsoft.Data.SqlClient;

namespace BlazorConsultant.Services;

/// <summary>
/// Service for ensuring database schema exists on application startup.
/// Creates SystemPrompts table if it doesn't exist (auto-migration).
/// </summary>
public class DatabaseSetupService
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseSetupService> _logger;

    public DatabaseSetupService(IConfiguration configuration, ILogger<DatabaseSetupService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration");
        _logger = logger;
    }

    /// <summary>
    /// Ensures the database schema exists. Creates tables if needed.
    /// Safe to call multiple times (idempotent).
    /// </summary>
    public async Task EnsureDatabaseSetupAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Check if SystemPrompts table exists
            var tableExists = await connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_NAME = 'SystemPrompts'") > 0;

            if (!tableExists)
            {
                _logger.LogInformation("SystemPrompts table not found. Creating database schema...");
                await CreateSchemaAsync(connection);
                _logger.LogInformation("Database schema created successfully");
            }
            else
            {
                _logger.LogInformation("Database schema already exists. Skipping creation.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure database setup");
            throw new InvalidOperationException("Database setup failed. Please check connection string and database permissions.", ex);
        }
    }

    private async Task CreateSchemaAsync(SqlConnection connection)
    {
        var sql = @"
-- SystemPrompts table for storing custom instructions
CREATE TABLE SystemPrompts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Content NVARCHAR(MAX) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDefault BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Indexes for performance
CREATE INDEX IX_SystemPrompts_IsActive ON SystemPrompts(IsActive);
CREATE INDEX IX_SystemPrompts_IsDefault ON SystemPrompts(IsDefault);

-- Ensure only one default prompt at a time
CREATE UNIQUE INDEX UIX_SystemPrompts_SingleDefault
    ON SystemPrompts(IsDefault)
    WHERE IsDefault = 1;

-- Schema version tracking table
CREATE TABLE SchemaVersion (
    Version INT NOT NULL PRIMARY KEY,
    AppliedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Description NVARCHAR(500) NULL
);

INSERT INTO SchemaVersion (Version, Description)
VALUES (1, 'Initial schema: SystemPrompts table with indexes');
";

        await connection.ExecuteAsync(sql);
        _logger.LogInformation("Executed schema creation script successfully");
    }
}
