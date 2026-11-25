# HospiKeeper.Domain · 领域模型

无框架依赖的领域层，包含实体、枚举与计费领域服务。不得引用 EF Core、Avalonia、ASP.NET Core 或其他业务模块。

## 内容

- `Entities/`：`Room`、`RoomType`、`AppUser`、`CheckInRecord`、`Reservation`、`OperationLog` 等；
- `Enums/`：房态 `Free / Occupied / Reserved / Cleaning / Repair / Disabled`，入住状态、预定状态、渠道、角色、账号状态等；
- 计费服务：按天 / 加钟计费、应收与押金差额计算；
- 领域异常基类与通用值对象。

## 业务规则

- 按天：不足一晚按一晚，跨 12:00 视为新一天（`ceil(时长/24h)`，至少 1 晚）。
- 加钟：小时数向上取整，房费 = `ceil(小时 × 成交单价 ÷ 24)`。
- 应收 = 房费 + 追加消费 + 损坏扣费；差额 = 应收 − 押金。
- 入住 `I`、结算 `S`、预定 `R`，格式 `前缀 + yyyyMMdd + 3 位序号`。
- 房态流转：入住 Free/Reserved → Occupied；退房 → Cleaning；保洁完成 / 恢复 → Free。
- 停用与维修房不计入可售房分母。

Domain 不处理数据库、HTTP、UI 或缓存；这些由 Application / Infrastructure / 各客户端负责。
