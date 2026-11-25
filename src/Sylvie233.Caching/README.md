# Sylvie233.Caching · 缓存端口

与业务无关的缓存抽象，支持 Memory / Redis 切换。HospiKeeper.Application 只依赖 `ICacheService`，不感知具体实现。

## 端口

```csharp
ICacheService
  GetAsync<T>(key)
  SetAsync<T>(key, value, ttl)
  SetPersistentAsync<T>(key, value)   // 无 TTL，用于系统配置
  GetOrCreateAsync<T>(key, factory, ttl)
  RemoveAsync(key)
  RemoveByPrefixAsync(prefix)
```

- `Memory`：`IMemoryCache` + key 注册表，适合单进程调试；
- `Redis`：`StackExchange.Redis`，支持多实例共享、前缀清理与 TTL；
- `GetOrCreateAsync` 内置进程内防击穿锁；
- Redis 异常时按缓存 miss 回源，不阻断业务；
- `SetPersistentAsync` 返回是否成功，失败时不静默成功。

## 切换

```json
{
  "Cache": {
    "Provider": "Redis",
    "RedisConnectionString": "localhost:6379",
    "RedisDatabase": 0,
    "InstanceName": "hospikeeper:"
  }
}
```

设置 `Cache:Provider=Memory` 或环境变量 `Cache__Provider=Memory` 即可切换。

## 业务约定

- 业务 key 只在 `HospiKeeper.Application/Caching/CacheKeys.cs` 定义；TTL 在 `CacheDurations.cs` 定义。
- 写操作成功后调用 `IHotelCacheInvalidator`：
  - 房间 / 房型 → `InvalidateRoomsAsync`
  - 入住 / 退房 / 预定 / 追加消费 → `InvalidateFrontDeskAsync`
- 不要在控制器中直接写缓存；读多写少接口在 Application 服务中调用缓存。

## Redis 运行

由 `Scripts/docker-compose.yml` 管理（`hospikeeper-redis`，`localhost:6379`）：

- 开启 AOF：`--appendonly yes --appendfsync everysec`；
- `--maxmemory-policy volatile-lru`：只淘汰带 TTL 的普通缓存；
- 系统配置 key `config:site` 无 TTL，通过 AOF 持久化。

修改 Redis 启动参数后需要重建容器：

```bash
docker compose -f Scripts/docker-compose.yml up -d --force-recreate redis
```
