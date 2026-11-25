using System.ComponentModel.DataAnnotations;

namespace HospiKeeper.Contracts;

/// <summary>同住人信息。</summary>
/// <param name="Name">同住人姓名。</param>
/// <param name="Phone">手机号，可为空。</param>
/// <param name="IdCard">身份证号，可为空。</param>
public sealed record CompanionInput(
    [param: Required(ErrorMessage = "请输入同住人姓名。")]
    [param: StringLength(32, ErrorMessage = "同住人姓名不能超过 32 个字符。")]
    string Name,
    [param: RegularExpression(ValidationPatterns.OptionalMobile, ErrorMessage = "同住人手机号格式无效。")]
    string? Phone,
    [param: RegularExpression(ValidationPatterns.OptionalIdCard, ErrorMessage = "同住人身份证号格式无效。")]
    string? IdCard);

/// <summary>办理入住的输入。</summary>
public sealed record CheckInRequest(
    [param: Range(1, int.MaxValue, ErrorMessage = "请选择房间。")]
    int RoomId,
    [param: Required(ErrorMessage = "请输入客人姓名。")]
    [param: StringLength(32, ErrorMessage = "客人姓名不能超过 32 个字符。")]
    string GuestName,
    [param: Required(ErrorMessage = "请输入手机号。")]
    [param: RegularExpression(ValidationPatterns.Mobile, ErrorMessage = "请输入 11 位有效手机号。")]
    string GuestPhone,
    [param: Required(ErrorMessage = "请输入身份证号。")]
    [param: RegularExpression(ValidationPatterns.IdCard, ErrorMessage = "请输入 18 位有效身份证号（末位可为 X）。")]
    string GuestIdCard,
    DateTime CheckInTime,
    DateTime PlanOutTime,
    [param: Range(typeof(decimal), "0.01", "99999999", ErrorMessage = "成交单价必须大于 0。")]
    decimal Price,
    [param: Range(typeof(decimal), "0", "99999999", ErrorMessage = "押金不能为负数。")]
    decimal Deposit,
    [param: StringLength(255, ErrorMessage = "备注不能超过 255 个字符。")]
    string? Note,
    [param: Range(1, int.MaxValue, ErrorMessage = "预定单号无效。")]
    int? ReservationId = null,
    List<CompanionInput>? Companions = null);

/// <summary>追加消费的输入（路径中已带入住单 ID）。</summary>
/// <param name="Item">消费项目。</param>
/// <param name="Amount">金额，必须大于 0。</param>
public sealed record ExtraChargeInput(
    [param: Required(ErrorMessage = "请输入消费项目。")]
    [param: StringLength(64, ErrorMessage = "消费项目不能超过 64 个字符。")]
    string Item,
    [param: Range(typeof(decimal), "0.01", "99999999", ErrorMessage = "消费金额必须大于 0。")]
    decimal Amount);

/// <summary>追加消费的服务层输入（含入住单 ID）。</summary>
/// <param name="CheckInId">入住单 ID。</param>
/// <param name="Item">消费项目。</param>
/// <param name="Amount">金额。</param>
public sealed record ExtraChargeRequest(int CheckInId, string Item, decimal Amount);
