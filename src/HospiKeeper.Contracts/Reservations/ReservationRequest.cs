using System.ComponentModel.DataAnnotations;

namespace HospiKeeper.Contracts;

/// <summary>创建预定的输入。</summary>
public sealed record ReservationRequest(
    [param: Required(ErrorMessage = "请输入客人姓名。")]
    [param: StringLength(32, ErrorMessage = "客人姓名不能超过 32 个字符。")]
    string GuestName,
    [param: Required(ErrorMessage = "请输入手机号。")]
    [param: RegularExpression(ValidationPatterns.Mobile, ErrorMessage = "请输入 11 位有效手机号。")]
    string GuestPhone,
    [param: Range(1, int.MaxValue, ErrorMessage = "请选择房间。")]
    int RoomId,
    DateTime ArriveTime,
    [param: StringLength(255, ErrorMessage = "备注不能超过 255 个字符。")]
    string? Note);
