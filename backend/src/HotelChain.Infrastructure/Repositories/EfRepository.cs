using HotelChain.Domain.Common;
using HotelChain.Domain.Repositories;
using HotelChain.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelChain.Infrastructure.Repositories;

/// <summary>Generic repository reusable for any entity.</summary>
public class EfRepository<T>(HotelChainDbContext db) : IRepository<T> where T : Entity
{
    protected HotelChainDbContext Db { get; } = db;
    protected DbSet<T> Set => Db.Set<T>();

    public virtual Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => Set.FirstOrDefaultAsync(e => e.Id == id, ct);

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        Set.Add(entity);
        await Db.SaveChangesAsync(ct);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        Set.Update(entity);
        await Db.SaveChangesAsync(ct);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        Set.Remove(entity);
        await Db.SaveChangesAsync(ct);
    }
}
