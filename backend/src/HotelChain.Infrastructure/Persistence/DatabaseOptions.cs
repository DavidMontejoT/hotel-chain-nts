namespace HotelChain.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>Sqlite | SqlServer | InMemory</summary>
    public string Provider { get; set; } = "Sqlite";

    /// <summary>Aplica las migraciones pendientes al iniciar la aplicación.</summary>
    public bool ApplyMigrations { get; set; }

    /// <summary>Inserta datos de ejemplo si la tabla está vacía.</summary>
    public bool Seed { get; set; }
}
