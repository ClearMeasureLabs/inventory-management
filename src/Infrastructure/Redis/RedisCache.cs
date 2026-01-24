using Application.Infrastructure;
using Domain.Entities;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Redis;

public class RedisCache : ICache
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase _database;
    private bool _disposed;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public RedisCache(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _database = connectionMultiplexer.GetDatabase();
    }

    public void Dispose()
    {
        _connectionMultiplexer.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<Container?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(key);

        if (value.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<Container>(value.ToString(), _jsonOptions);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _database.KeyDeleteAsync(key);
    }

    public async Task SetAsync(string key, Container value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        await _database.StringSetAsync(key, json, expiration);
    }
}
