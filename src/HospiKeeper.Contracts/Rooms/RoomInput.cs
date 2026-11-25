using System.ComponentModel.DataAnnotations;

namespace HospiKeeper.Contracts;

/// <summary>创建 / 修改房间的输入。</summary>
public sealed record RoomInput(
    [param: Required(ErrorMessage = "请输入房号。")]
    [param: StringLength(16, ErrorMessage = "房号不能超过 16 个字符。")]
    string RoomNo,
    [param: Range(1, 99, ErrorMessage = "楼层需在 1 - 99 之间。")]
    int Floor,
    [param: Range(1, int.MaxValue, ErrorMessage = "请选择房型。")]
    int TypeId,
    [param: Range(1, 10, ErrorMessage = "床位数需在 1 - 10 之间。")]
    int Beds,
    [param: Range(typeof(decimal), "0.01", "99999999", ErrorMessage = "标价必须大于 0。")]
    decimal Price,
    [param: Range(typeof(decimal), "0", "99999999", ErrorMessage = "底价不能为负数。")]
    decimal MinPrice,
    [param: Range(typeof(decimal), "0", "99999999", ErrorMessage = "押金不能为负数。")]
    decimal Deposit,
    bool HasWindow,
    bool HasBath,
    [param: StringLength(255, ErrorMessage = "备注不能超过 255 个字符。")]
    string? Note);

/// <summary>创建 / 修改房型的输入。</summary>
public sealed record RoomTypeInput(
    [param: Required(ErrorMessage = "请输入房型名称。")]
    [param: StringLength(32, ErrorMessage = "房型名称不能超过 32 个字符。")]
    string Name,
    [param: Range(typeof(decimal), "0", "99999999", ErrorMessage = "基础定价不能为负数。")]
    decimal BasePrice,
    [param: StringLength(255, ErrorMessage = "备注不能超过 255 个字符。")]
    string? Note);

/// <summary>设置维修 / 停用状态。</summary>
/// <param name="Status">目标房态，仅允许维修或停用。</param>
/// <param name="Note">原因备注。</param>
public sealed record MaintenanceRequest(
    HospiKeeper.Domain.Enums.RoomStatus Status,
    [param: StringLength(255, ErrorMessage = "备注不能超过 255 个字符。")]
    string? Note);
