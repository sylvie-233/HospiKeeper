namespace HospiKeeper.Domain.Enums;

/// <summary>操作员角色。</summary>
public enum UserRole
{
    Front = 0,
    Admin = 1
}

/// <summary>账号状态。</summary>
public enum AccountStatus
{
    Active = 0,
    Disabled = 1
}

/// <summary>房态生命周期：空闲 / 已入住 / 预定 / 打扫中 / 维修 / 停用。</summary>
public enum RoomStatus
{
    Free = 0,
    Occupied = 1,
    Reserved = 2,
    Cleaning = 3,
    Repair = 4,
    Disabled = 5
}

/// <summary>预定状态。</summary>
public enum ReservationStatus
{
    Pending = 0,
    Converted = 1,
    Cancelled = 2
}

/// <summary>入住单状态。</summary>
public enum CheckInStatus
{
    Living = 0,
    CheckedOut = 1
}

/// <summary>退房计费方式：按天 / 加钟（按小时折算）。</summary>
public enum BillingMode
{
    Daily = 0,
    Hourly = 1
}
