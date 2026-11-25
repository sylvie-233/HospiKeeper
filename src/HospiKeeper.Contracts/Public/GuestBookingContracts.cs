using System.ComponentModel.DataAnnotations;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Contracts;

/// <summary>游客端房型可订量。</summary>
/// <param name="TypeId">房型 ID。</param>
/// <param name="Name">房型名称。</param>
/// <param name="BasePrice">每晚基础价。</param>
/// <param name="Note">房型简介。</param>
/// <param name="Beds">床位数。</param>
/// <param name="HasWindow">是否有窗。</param>
/// <param name="HasBath">是否带独立卫浴。</param>
/// <param name="SellableRooms">可售房数（总房数 − 维修 − 停用）。</param>
/// <param name="Remaining">当前日期段剩余可订数。</param>
public sealed record GuestRoomTypeDto(
    int TypeId,
    string Name,
    decimal BasePrice,
    string? Note,
    int Beds,
    bool HasWindow,
    bool HasBath,
    int SellableRooms,
    int Remaining);

/// <summary>游客查询日期段的房型可订结果。</summary>
public sealed record GuestAvailabilityResult(
    DateTime CheckIn,
    DateTime CheckOut,
    int Nights,
    IReadOnlyList<GuestRoomTypeDto> RoomTypes);

/// <summary>首页统计数据。</summary>
public sealed record HotelSummaryDto(
    int RoomCount,
    int TypeCount,
    double Rating,
    int WalkToLakeMinutes);

/// <summary>游客提交在线预订。</summary>
public sealed record GuestBookingRequest(
    [param: Range(1, int.MaxValue, ErrorMessage = "请选择房型。")]
    int TypeId,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    [param: Required(ErrorMessage = "请输入入住人姓名。")]
    [param: StringLength(20, ErrorMessage = "入住人姓名不能超过 20 个字符。")]
    string GuestName,
    [param: Required(ErrorMessage = "请输入手机号。")]
    [param: RegularExpression(ValidationPatterns.Mobile, ErrorMessage = "请输入 11 位有效手机号。")]
    string GuestPhone,
    [param: RegularExpression(ValidationPatterns.OptionalIdCard, ErrorMessage = "身份证号格式无效。")]
    string? GuestIdCard,
    [param: StringLength(255, ErrorMessage = "备注不能超过 255 个字符。")]
    string? Note);

/// <summary>游客端预订记录。</summary>
public sealed record GuestReservationDto(
    string ResNo,
    int TypeId,
    string RoomTypeName,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int Nights,
    decimal UnitPrice,
    decimal Total,
    ReservationStatus Status,
    string? RoomNo,
    string GuestName,
    string GuestPhone,
    ReservationChannel Channel,
    DateTime CreatedAt);

/// <summary>游客在线取消预订（手机号即凭证）。</summary>
public sealed record CancelGuestBookingRequest(
    [param: Required(ErrorMessage = "请输入手机号。")]
    [param: RegularExpression(ValidationPatterns.Mobile, ErrorMessage = "请输入 11 位有效手机号。")]
    string Phone);
