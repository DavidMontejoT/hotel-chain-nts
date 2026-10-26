namespace HotelChain.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>Sqlite | SqlServer | InMemory</summary>
    public string Provider { get; set; } = "Sqlite";

    /// <summary>Crea el esquema al iniciar si no existe.</summary>
    public bool EnsureCreated { get; set; } = true;

    /// <summary>Inserta datos de ejemplo si la tabla está vacía.</summary>
    public bool Seed { get; set; }
}
