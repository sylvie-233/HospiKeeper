using HospiKeeper.Application.Caching;
using Sylvie233.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace HospiKeeper.Tests;

public sealed class CacheServiceTests
{
    [Fact]
    public async Task SetAndGet_RoundTripsValue()
    {
        var cache = CreateCache();
        var key = CacheKeys.Rooms(null, null, null);

        await cache.SetAsync(key, new List<string> { "101", "102" }, TimeSpan.FromMinutes(1));
        var value = await cache.GetAsync<List<string>>(key);

        Assert.NotNull(value);
        Assert.Equal(2, value!.Count);
    }

    [Fact]
    public async Task GetOrCreate_ExecutesFactoryOnlyOnMiss()
    {
        var cache = CreateCache();
        var calls = 0;

        var first = await cache.GetOrCreateAsync(
            "factory-key",
            _ =>
            {
                calls++;
                return Task.FromResult(new List<int> { 1, 2, 3 });
            },
            TimeSpan.FromMinutes(1));
        var second = await cache.GetOrCreateAsync(
            "factory-key",
            _ =>
            {
                calls++;
                return Task.FromResult(new List<int> { 9 });
            },
            TimeSpan.FromMinutes(1));

        Assert.Equal(1, calls);
        Assert.Equal(3, first.Count);
        Assert.Equal(3, second.Count);
    }

    [Fact]
    public async Task RemoveByPrefix_RemovesMatchingEntriesOnly()
    {
        var cache = CreateCache();
        await cache.SetAsync("rooms:a", new List<int> { 1 });
        await cache.SetAsync("rooms:b", new List<int> { 2 });
        await cache.SetAsync("reports:snapshot", new List<int> { 3 });

        await cache.RemoveByPrefixAsync("rooms:");

        Assert.Null(await cache.GetAsync<List<int>>("rooms:a"));
        Assert.Null(await cache.GetAsync<List<int>>("rooms:b"));
        Assert.NotNull(await cache.GetAsync<List<int>>("reports:snapshot"));
    }

    private static MemoryCacheService CreateCache()
    {
        var options = Options.Create(new CacheOptions
        {
            Provider = CacheProvider.Memory,
            InstanceName = "hospikeeper-tests:",
            DefaultExpirationSeconds = 60
        });

        return new MemoryCacheService(
            new MemoryCache(new MemoryCacheOptions()),
            options,
            NullLogger<MemoryCacheService>.Instance);
    }
}
