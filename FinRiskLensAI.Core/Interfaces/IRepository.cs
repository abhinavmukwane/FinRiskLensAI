using FinRiskLensAI.Core.Models.Common;
using System.Linq.Expressions;

namespace FinRiskLensAI.Core.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<T> AddAsync(T entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
        void Update(T entity);
        void Remove(T entity);
        Task<int> CountAsync(CancellationToken ct = default);
    }
}
