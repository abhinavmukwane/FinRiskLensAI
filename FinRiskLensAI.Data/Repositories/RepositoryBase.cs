using FinRiskLensAI.Core.Interfaces;
using FinRiskLensAI.Data.DbContextEDMX;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FinRiskLensAI.Data.Repositories
{
    public class RepositoryBase<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext Db;
        protected DbSet<T> Set => Db.Set<T>();

        public RepositoryBase(ApplicationDbContext db) => Db = db;

        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
            => await Set.FindAsync(new object[] { id }, ct);

        public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
            => await Set.ToListAsync(ct);

        public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
            => await Set.Where(predicate).ToListAsync(ct);

        public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
        {
            await Set.AddAsync(entity, ct);
            return entity;
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
            => await Set.AddRangeAsync(entities, ct);

        public virtual void Update(T entity) => Set.Update(entity);

        public virtual void Remove(T entity) => Set.Remove(entity);

        public virtual async Task<int> CountAsync(CancellationToken ct = default)
            => await Set.CountAsync(ct);
    }
}
