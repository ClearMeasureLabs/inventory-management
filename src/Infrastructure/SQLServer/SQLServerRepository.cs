using Application.Infrastructure;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SQLServer;

public class SQLServerRepository : IRepository
{
    private readonly InventoryDbContext _context;
    private bool _disposed;

    public SQLServerRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public DbSet<Container> Containers => _context.Containers;

    public DbSet<Item> Items => _context.Items;

    public DbSet<WorkOrder> WorkOrders => _context.WorkOrders;

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
