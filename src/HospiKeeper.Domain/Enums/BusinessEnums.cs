namespace HospiKeeper.Domain.Enums;

/// <summary>操作员角色。</summary>
public enum UserRole
{
    /// <summary>前台：办理入住 / 退房 / 预定，只能查看本人经办的数据。</summary>
    Front = 0,

    /// <summary>管理员：拥有前台全部权限，并可管理房间、房型与账号。</summary>
    Admin = 1
}

/// <summary>账号状态。</summary>
public enum AccountStatus
{
    /// <summary>启用，可正常登录。</summary>
    Active = 0,

    /// <summary>停用，登录与鉴权都会被拒绝。</summary>
    Disabled = 1
}

/// <summary>房态生命周期：空闲 / 已入住 / 预定 / 打扫中 / 维修 / 停用。</summary>
public enum RoomStatus
{
    /// <summary>空闲，可办理入住或预定。</summary>
    Free = 0,

    /// <summary>已入住。</summary>
    Occupied = 1,

    /// <summary>已被预定，等待客人到店。</summary>
    Reserved = 2,

    /// <summary>退房后待打扫。</summary>
    Cleaning = 3,

    /// <summary>维修中，不计入可售房。</summary>
    Repair = 4,

    /// <summary>停用，不计入可售房。</summary>
    Disabled = 5
}

/// <summary>预定状态。</summary>
public enum ReservationStatus
{
    /// <summary>待到店。</summary>
    Pending = 0,

    /// <summary>已转入住。</summary>
    Converted = 1,

    /// <summary>已取消。</summary>
    Cancelled = 2
}

/// <summary>入住单状态。</summary>
public enum CheckInStatus
{
    /// <summary>在住。</summary>
    Living = 0,

    /// <summary>已退房。</summary>
    CheckedOut = 1
}

/// <summary>预订渠道。</summary>
public enum ReservationChannel
{
    /// <summary>前台代客预订（已指定房间）。</summary>
    WalkIn = 0,

    /// <summary>游客在线预订（只锁房型与日期段，到店分配房间）。</summary>
    Web = 1
}

/// <summary>退房计费方式：按天 / 加钟（按小时折算）。</summary>
public enum BillingMode
{
    /// <summary>按天计费，不足一晚按一晚。</summary>
    Daily = 0,

    /// <summary>加钟计费，小时数向上取整后按 24 小时折算。</summary>
    Hourly = 1
}
