# HospiKeeper.Tests · 业务测试

xUnit + EF Core InMemory 测试工程。当前 38 个用例，覆盖计费、权限、入住 / 退房 / 预定、追加消费、报表、缓存、契约序列化、游客库存与房型改价同步。

## 运行

```bash
dotnet test tests/HospiKeeper.Tests/HospiKeeper.Tests.csproj
```

## ServiceTestContext

`ServiceTestContext.CreateAsync()`：

- 创建 InMemory `HotelDbContext`；
- 调用 `DemoDataSeeder.SeedAsync` 生成完整演示数据；
- 构造 `SessionContext`、`EfUnitOfWork`、缓存、日志、全部 Application 服务；
- 默认以 `admin` 登录（密码 `123456`）。

测试不要连接真实 MySQL；需要集成验证时启动 API 做 curl / 手工冒烟。

## 新增测试

- 新增业务规则必须补测试；
- 涉及房型改价时同时断言同房型房间 `Price` 已同步；
- 涉及缓存失效时断言写操作后 key 被移除 / 回源；
- 测试使用 `await using var ctx = await ServiceTestContext.CreateAsync();`。
