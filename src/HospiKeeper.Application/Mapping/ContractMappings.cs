using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;

namespace HospiKeeper.Application.Mapping;

/// <summary>
/// 领域实体 → API 契约的映射。应用层负责出口转换，控制器不再直接返回 EF 实体。
/// </summary>
public static class ContractMappings
{
    /// <summary>操作员账号 → 对外视图。</summary>
    public static UserDto ToDto(this AppUser user) => new(
        user.Id,
        user.Username,
        user.RealName,
        user.Role,
        user.Status,
        user.LastLogin,
        user.DisplayName,
        user.IsAdmin);

    /// <summary>房间 → 对外视图。</summary>
    public static RoomDto ToDto(this Room room) => new(
        room.Id,
        room.RoomNo,
        room.Floor,
        room.TypeId,
        room.TypeName,
        room.Beds,
        room.Price,
        room.MinPrice,
        room.Deposit,
        room.HasWindow,
        room.HasBath,
        room.Status,
        room.Note,
        room.CreatedAt,
        room.IsSellable);

    /// <summary>房型 → 对外视图。</summary>
    public static RoomTypeDto ToDto(this RoomType roomType) => new(
        roomType.Id,
        roomType.Name,
        roomType.BasePrice,
        roomType.Note,
        roomType.CreatedAt);

    /// <summary>预定 → 对外视图。</summary>
    public static ReservationDto ToDto(this Reservation reservation) => new(
        reservation.Id,
        reservation.ResNo,
        reservation.GuestName,
        reservation.GuestPhone,
        reservation.RoomId,
        reservation.RoomNo,
        reservation.RoomTypeName,
        reservation.ArriveTime,
        reservation.Status,
        reservation.CheckInId,
        reservation.CheckInNo,
        reservation.Note,
        reservation.OpUserId,
        reservation.OperatorName,
        reservation.CreatedAt);

    /// <summary>入住记录 → 对外视图，包含同住人与消费明细。</summary>
    public static CheckInRecordDto ToDto(this CheckInRecord record) => new(
        record.Id,
        record.CheckInNo,
        record.RoomId,
        record.RoomNo,
        record.RoomTypeName,
        record.GuestName,
        record.GuestPhone,
        record.GuestIdCard,
        record.CheckInTime,
        record.PlanOutTime,
        record.ActualOutTime,
        record.Price,
        record.OriginPrice,
        record.Deposit,
        record.ExtendHours,
        record.Status,
        record.Note,
        record.OpUserId,
        record.OperatorName,
        record.CreatedAt,
        record.Companions.Select(c => c.ToDto()).ToList(),
        record.ExtraCharges.Select(c => c.ToDto()).ToList(),
        record.ExtraTotal);

    /// <summary>同住人 → 对外视图。</summary>
    public static CompanionDto ToDto(this Companion companion) => new(
        companion.Id,
        companion.CheckInId,
        companion.Name,
        companion.Phone,
        companion.IdCard);

    /// <summary>追加消费 → 对外视图。</summary>
    public static ExtraChargeDto ToDto(this ExtraCharge charge) => new(
        charge.Id,
        charge.CheckInId,
        charge.Item,
        charge.Amount,
        charge.ChargeTime,
        charge.OpUserId,
        charge.OperatorName);

    /// <summary>结算账单 → 对外视图。</summary>
    public static CheckoutBillDto ToDto(this CheckoutBill bill) => new(
        bill.Mode,
        bill.Id,
        bill.BillNo,
        bill.CheckInId,
        bill.CheckInNo,
        bill.RoomId,
        bill.RoomNo,
        bill.RoomTypeName,
        bill.GuestName,
        bill.CheckInTime,
        bill.CheckoutTime,
        bill.Nights,
        bill.RoomFee,
        bill.ExtraTotal,
        bill.Damage,
        bill.OtherFee,
        bill.DepositReceived,
        bill.DepositUsed,
        bill.Total,
        bill.Diff,
        bill.Note,
        bill.OpUserId,
        bill.OperatorName,
        bill.CreatedAt,
        bill.Extras.Select(e => e.ToDto()).ToList());

    /// <summary>账单明细 → 对外视图。</summary>
    public static BillLineDto ToDto(this BillLine line) => new(
        line.Id,
        line.CheckoutBillId,
        line.Item,
        line.Amount);

    /// <summary>操作日志 → 对外视图。</summary>
    public static OperationLogDto ToDto(this OperationLog log) => new(
        log.Id,
        log.UserId,
        log.UserName,
        log.Action,
        log.Target,
        log.Detail,
        log.BizNo,
        log.CreatedAt);
}
