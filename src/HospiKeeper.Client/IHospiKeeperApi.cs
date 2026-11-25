using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;
using HospiKeeper.Domain.Services;

namespace HospiKeeper.Client;

/// <summary>桌面端访问 HospiKeeper.Api 的契约。</summary>
public interface IHospiKeeperApi
{
    UserDto? CurrentUser { get; }

    Task<LoginResponse> LoginAsync(string username, string password, CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);

    Task<UserDto> GetMeAsync(CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<UserDto> CreateUserAsync(AccountInput input, CancellationToken cancellationToken = default);

    Task UpdateUserAsync(int userId, AccountInput input, CancellationToken cancellationToken = default);

    Task SetUserStatusAsync(int userId, AccountStatus status, CancellationToken cancellationToken = default);

    Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Room>> GetRoomsAsync(
        int? floor = null,
        int? typeId = null,
        RoomStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Room>> GetFreeRoomsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetFloorsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoomType>> GetRoomTypesAsync(CancellationToken cancellationToken = default);

    Task<Room> CreateRoomAsync(RoomInput input, CancellationToken cancellationToken = default);

    Task UpdateRoomAsync(int roomId, RoomInput input, CancellationToken cancellationToken = default);

    Task DeleteRoomAsync(int roomId, CancellationToken cancellationToken = default);

    Task MarkCleaningCompleteAsync(int roomId, CancellationToken cancellationToken = default);

    Task RestoreRoomAsync(int roomId, CancellationToken cancellationToken = default);

    Task SetMaintenanceAsync(int roomId, RoomStatus status, string? note, CancellationToken cancellationToken = default);

    Task<RoomType> CreateRoomTypeAsync(RoomTypeInput input, CancellationToken cancellationToken = default);

    Task UpdateRoomTypeAsync(int roomTypeId, RoomTypeInput input, CancellationToken cancellationToken = default);

    Task DeleteRoomTypeAsync(int roomTypeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BoardRoom>> GetBoardAsync(CancellationToken cancellationToken = default);

    Task<FrontDeskCounts> GetCountsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StayInfo>> GetLivingStaysAsync(string? keyword = null, CancellationToken cancellationToken = default);

    Task<StayInfo?> GetStayAsync(int checkInId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Reservation>> GetReservationsAsync(
        ReservationStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<Reservation?> GetReservationAsync(int reservationId, CancellationToken cancellationToken = default);

    Task<CheckInRecord> CheckInAsync(CheckInRequest request, CancellationToken cancellationToken = default);

    Task<BillingResult> QuoteCheckoutAsync(
        int checkInId,
        CheckoutQuoteRequest request,
        CancellationToken cancellationToken = default);

    Task<CheckoutBill> CheckoutAsync(CheckoutRequest request, CancellationToken cancellationToken = default);

    Task AddExtraChargeAsync(int checkInId, string item, decimal amount, CancellationToken cancellationToken = default);

    Task<Reservation> CreateReservationAsync(ReservationRequest request, CancellationToken cancellationToken = default);

    Task CancelReservationAsync(int reservationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CheckInRecord>> QueryCheckInsAsync(RecordQuery query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CheckoutBill>> QueryBillsAsync(RecordQuery query, CancellationToken cancellationToken = default);

    Task<CheckInRecord?> GetCheckInAsync(int checkInId, CancellationToken cancellationToken = default);

    Task<CheckoutBill?> GetBillAsync(int billId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OperationLog>> QueryLogsAsync(
        string? action = null,
        string? keyword = null,
        CancellationToken cancellationToken = default);

    Task<ReportSnapshot> GetReportSnapshotAsync(CancellationToken cancellationToken = default);

    Task<RevenueRangeResult> GetRevenueRangeAsync(
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default);
}
