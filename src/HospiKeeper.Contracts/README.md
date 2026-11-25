# HospiKeeper.Contracts · API 契约

桌面端、游客 Web、Android App 与 API 共享的 DTO / 请求模型 / 动作常量层。只依赖 Domain 中的枚举等基础类型，不依赖 EF Core / ASP.NET Core。

## 目录

```text
Auth/          登录、用户、改密、账号
Rooms/         房间 / 房型输入与 DTO
FrontDesk/     入住、退房、看板、在住 DTO
Reservations/  预定输入与 DTO
Records/       记录查询与操作日志 DTO
Reports/       报表快照与区间营收
Common/        错误响应、健康检查、动作常量、校验正则
```

命名空间统一为 `HospiKeeper.Contracts`。

## 约定

- 请求类型以 `Request` / `Input` 结尾，响应类型以 `Dto` 结尾；
- 不返回 Domain 实体；
- record 的校验特性标在构造参数上：`[param: Required(ErrorMessage = "...")] string Name`；
  `[property:]` 会被 MVC 忽略并抛异常；
- DataAnnotations 只做入口格式 / 必填 / 范围约束，跨字段、唯一性、状态流转在 Application 用 `InputGuard` 校验；
- 实体 → DTO 映射集中在 `Application/Mapping/ContractMappings.cs`；
- 修改 DTO 会同时影响 API、Client、Desktop、Web 与 App，必须同步更新并回归各端。

## 动作常量

`Common/OperationActions.cs` 统一维护登录、入住、退房、预定、取消预定、改价等操作日志动作名，业务与前端不要硬编码字符串。
