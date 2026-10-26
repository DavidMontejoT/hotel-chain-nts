namespace HotelChain.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>Sqlite | SqlServer | InMemory</summary>
    public string Provider { get; set; } = "Sqlite";

    /// <summary>Applies pending migrations on startup.</summary>
    public bool ApplyMigrations { get; set; }

    /// <summary>Inserts sample hotels when the table is empty.</summary>
    public bool Seed { get; set; }
}
