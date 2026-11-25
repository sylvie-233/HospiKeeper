using HospiKeeper.Contracts;
using HospiKeeper.Domain.Enums;
using HospiKeeper.Domain.Services;

namespace HospiKeeper.Client;

/// <summary>
/// 桌面端访问 HospiKeeper.Api 的契约。所有方法都通过 HTTP + Bearer Token 调用，
/// 不直接接触数据库或业务实现。
/// </summary>
public interface IHospiKeeperApi
{
    /// <summary>当前登录用户；未登录时为 <c>null</c>。</summary>
    UserDto? CurrentUser { get; }

    /// <summary>登录并保存令牌。</summary>
    Task<LoginResponse> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>退出登录并清除本地令牌。</summary>
    Task LogoutAsync(CancellationToken cancellationToken = default);

    /// <summary>从服务端刷新当前用户信息。</summary>
    Task<UserDto> GetMeAsync(CancellationToken cancellationToken = default);

    /// <summary>修改当前账号密码。</summary>
    Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>查询全部账号（仅管理员）。</summary>
    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>创建账号（仅管理员）。</summary>
    Task<UserDto> CreateUserAsync(AccountInput input, CancellationToken cancellationToken = default);

    /// <summary>修改账号（仅管理员）。</summary>
    Task UpdateUserAsync(
        int userId,
        AccountInput input,
        CancellationToken cancellationToken = default);

    /// <summary>启用 / 停用账号（仅管理员）。</summary>
    Task SetUserStatusAsync(
        int userId,
        AccountStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>删除账号（仅管理员）。</summary>
    Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>按楼层、房型、房态查询房间。</summary>
    Task<IReadOnlyList<RoomDto>> GetRoomsAsync(
        int? floor = null,
        int? typeId = null,
        RoomStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>查询空闲房间。</summary>
    Task<IReadOnlyList<RoomDto>> GetFreeRoomsAsync(CancellationToken cancellationToken = default);

    /// <summary>查询已有楼层。</summary>
    Task<IReadOnlyList<int>> GetFloorsAsync(CancellationToken cancellationToken = default);

    /// <summary>查询全部房型。</summary>
    Task<IReadOnlyList<RoomTypeDto>> GetRoomTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>创建房间。</summary>
    Task<RoomDto> CreateRoomAsync(RoomInput input, CancellationToken cancellationToken = default);

    /// <summary>修改房间。</summary>
    Task UpdateRoomAsync(int roomId, RoomInput input, CancellationToken cancellationToken = default);

    /// <summary>删除房间。</summary>
    Task DeleteRoomAsync(int roomId, CancellationToken cancellationToken = default);

    /// <summary>标记保洁完成。</summary>
    Task MarkCleaningCompleteAsync(int roomId, CancellationToken cancellationToken = default);

    /// <summary>把维修 / 停用房间恢复为空闲。</summary>
    Task RestoreRoomAsync(int roomId, CancellationToken cancellationToken = default);

    /// <summary>设置维修或停用状态。</summary>
    Task SetMaintenanceAsync(
        int roomId,
        RoomStatus status,
        string? note,
        CancellationToken cancellationToken = default);

    /// <summary>创建房型。</summary>
    Task<RoomTypeDto> CreateRoomTypeAsync(RoomTypeInput input, CancellationToken cancellationToken = default);

    /// <summary>修改房型。</summary>
    Task UpdateRoomTypeAsync(
        int roomTypeId,
        RoomTypeInput input,
        CancellationToken cancellationToken = default);

    /// <summary>删除房型。</summary>
    Task DeleteRoomTypeAsync(int roomTypeId, CancellationToken cancellationToken = default);

    /// <summary>获取房态看板。</summary>
    Task<IReadOnlyList<BoardRoom>> GetBoardAsync(CancellationToken cancellationToken = default);

    /// <summary>获取前台汇总计数。</summary>
    Task<FrontDeskCounts> GetCountsAsync(CancellationToken cancellationToken = default);

    /// <summary>查询在住列表。</summary>
    Task<IReadOnlyList<StayInfo>> GetLivingStaysAsync(
        string? keyword = null,
        CancellationToken cancellationToken = default);

    /// <summary>获取在住详情；不存在时返回 <c>null</c>。</summary>
    Task<StayInfo?> GetStayAsync(int checkInId, CancellationToken cancellationToken = default);

    /// <summary>查询预定列表。</summary>
    Task<IReadOnlyList<ReservationDto>> GetReservationsAsync(
        ReservationStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>获取预定详情；不存在时返回 <c>null</c>。</summary>
    Task<ReservationDto?> GetReservationAsync(
        int reservationId,
        CancellationToken cancellationToken = default);

    /// <summary>办理入住。</summary>
    Task<CheckInRecordDto> CheckInAsync(
        CheckInRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>试算退房账单。</summary>
    Task<BillingResult> QuoteCheckoutAsync(
        int checkInId,
        CheckoutQuoteRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>办理退房。</summary>
    Task<CheckoutBillDto> CheckoutAsync(
        CheckoutRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>追加消费。</summary>
    Task AddExtraChargeAsync(
        int checkInId,
        string item,
        decimal amount,
        CancellationToken cancellationToken = default);

    /// <summary>创建预定。</summary>
    Task<ReservationDto> CreateReservationAsync(
        ReservationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>取消预定。</summary>
    Task CancelReservationAsync(int reservationId, CancellationToken cancellationToken = default);

    /// <summary>查询入住记录。</summary>
    Task<IReadOnlyList<CheckInRecordDto>> QueryCheckInsAsync(
        RecordQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>查询结算账单。</summary>
    Task<IReadOnlyList<CheckoutBillDto>> QueryBillsAsync(
        RecordQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>获取入住记录详情；不存在时返回 <c>null</c>。</summary>
    Task<CheckInRecordDto?> GetCheckInAsync(int checkInId, CancellationToken cancellationToken = default);

    /// <summary>获取账单详情；不存在时返回 <c>null</c>。</summary>
    Task<CheckoutBillDto?> GetBillAsync(int billId, CancellationToken cancellationToken = default);

    /// <summary>查询操作日志。</summary>
    Task<IReadOnlyList<OperationLogDto>> QueryLogsAsync(
        string? action = null,
        string? keyword = null,
        CancellationToken cancellationToken = default);

    /// <summary>获取报表概览。</summary>
    Task<ReportSnapshot> GetReportSnapshotAsync(CancellationToken cancellationToken = default);

    /// <summary>查询区间营收。</summary>
    Task<RevenueRangeResult> GetRevenueRangeAsync(
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default);
}
