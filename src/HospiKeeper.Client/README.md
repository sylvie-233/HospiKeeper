# HospiKeeper.Client · API 客户端

手写 `HttpClient` 封装，供桌面端、游客 Web 与 Android App 复用。只依赖 `Contracts` / `Domain`，不依赖 EF Core 或具体 UI。

## 使用

```csharp
var api = new HospiKeeperApiClient(new HttpClient
{
    BaseAddress = new Uri("http://localhost:5080/")
});

var login = await api.LoginAsync("admin", "123456");
var board = await api.GetBoardAsync();
```

登录成功后客户端保存 Bearer Token，后续请求自动附加 `Authorization`。Token 是 API 内存会话（12 小时）；客户端重启后需要重新登录。

## 接口范围

- 登录 / 登出 / 当前用户；
- 看板、房态汇总、在住、房间、房型；
- 入住、退房、追加消费、加钟、打扫 / 维修 / 恢复；
- 预定创建 / 查询 / 取消 / 转入住；
- 记录、操作日志、报表快照与区间营收；
- 系统配置、账号管理、游客公开接口。

`IHospiKeeperApi` 是各端的接入点；修改 DTO 时三端必须同步更新并跑通。

## 各端地址

- 桌面 / Web：`http://localhost:5080`
- 标准 Android 模拟器：`http://10.0.2.2:5080`
- LDPlayer / 真机：宿主机网关或局域网 IP，App 登录页可修改

Android 本地调试需要允许 cleartext HTTP。
