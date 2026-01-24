using Domain.Entities;

namespace Application.Infrastructure;

public interface ICache : IDisposable
{
    Task<Container?> GetAsync(string key, CancellationToken cancellationToken = default);

    Task SetAsync(string key, Container value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
