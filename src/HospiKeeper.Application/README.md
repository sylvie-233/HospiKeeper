# HospiKeeper.Application · 用例层

承载业务用例、仓储抽象、工作单元、缓存协调与实体 → DTO 映射。API 只做参数绑定和调用，不直接写业务规则；客户端不引用该模块。

## 结构

```text
Abstractions/
├─ Services/       用例接口（IAuthService、IBookingService...）
├─ Repositories/   仓储抽象
└─ IUnitOfWork
Services/          用例实现
Mapping/           ContractMappings.cs
Caching/           CacheKeys / CacheDurations
Common/            InputGuard / SessionContext / IClock
```

## 约定

- 接口位于 `Abstractions/Services`，实现位于 `Services`；不要在实现文件里重复声明接口。
- 依赖统一通过 `AddApplication()` 注册。
- 业务服务只依赖 `IUnitOfWork`，不出现 `DbContext` / EF API。
- 返回 Contracts DTO；仓储返回领域实体，映射在应用层完成。
- 输入校验统一用 `InputGuard`；业务错误抛 `DomainException`，权限错误抛 `ForbiddenException`。
- 依赖 `Sylvie233.Caching` 的 `ICacheService`；写操作成功后调用 `IHotelCacheInvalidator`。
- 读多写少接口在应用服务中做缓存，不在控制器写缓存。
- 时间统一通过 `IClock`，除种子 / 展示文案外避免 `DateTime.Now`。

## 业务边界

入住、退房、预定、追加消费、房态等写操作都由本层完成；游客预订库存计算集中在 `GuestBookingService`，Android 管理端的取消预定与快捷改价也复用这里的用例。
