namespace HospiKeeper.Contracts;

/// <summary>
/// 操作日志动作类型。与原型文案保持一致，前端筛选、后端写日志共用同一组常量。
/// </summary>
public static class OperationActions
{
    public const string CheckIn = "开房";
    public const string CheckOut = "退房";
    public const string Reservation = "预定";
    public const string CancelReservation = "取消预定";
    public const string Cleaning = "打扫";
    public const string ModifyPrice = "改价";
    public const string Login = "登录";
    public const string Logout = "退出";
    public const string ExtraCharge = "追加消费";
    public const string Restore = "恢复";
    public const string Room = "房间";
    public const string RoomType = "房型";
    public const string Account = "账号";
    public const string Password = "修改密码";
    public const string Maintenance = "维修";
    public const string Disable = "停用";

    /// <summary>日志页面提供的全部动作筛选项。</summary>
    public static readonly string[] All =
    [
        CheckIn, CheckOut, Reservation, CancelReservation, Cleaning, ModifyPrice,
        Login, Logout, ExtraCharge, Restore, Maintenance, Disable,
        Room, RoomType, Account, Password
    ];
}
