using Lyra.Onboarding.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Lyra.Onboarding.Services;

/// <summary>
/// Orchard Core's SQL Server provider does not create the database itself (confirmed the hard way
/// in Phase 0 — tenant setup fails with "Cannot open database" otherwise), so a self-service signup
/// has to run `CREATE DATABASE` up front, the same way the Docker Compose `db-init` step does for
/// the pre-provisioned tenants.
/// </summary>
public sealed class TenantDatabaseInitializer(IOptions<OnboardingOptions> options)
{
    public async Task CreateDatabaseAsync(string databaseName, CancellationToken ct = default)
    {
        var settings = options.Value;
        await using var connection = new SqlConnection(settings.MasterConnectionString);
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        // Database names come only from the slugified, regex-validated Subdomain field, never
        // raw user input, so string interpolation here is not attacker-controlled — CREATE DATABASE
        // also doesn't support parameterized identifiers.
        command.CommandText = $"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{databaseName}') CREATE DATABASE [{databaseName}]";
        await command.ExecuteNonQueryAsync(ct);
    }
}
