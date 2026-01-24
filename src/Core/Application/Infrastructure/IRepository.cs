using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Infrastructure;

public interface IRepository : IDisposable
{
    DbSet<Container> Containers { get; }

    DbSet<Item> Items { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
