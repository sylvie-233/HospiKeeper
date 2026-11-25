using System.ComponentModel.DataAnnotations;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Contracts;

/// <summary>办理退房的输入。</summary>
public sealed record CheckoutRequest(
    [param: Range(1, int.MaxValue, ErrorMessage = "入住单号无效。")]
    int CheckInId,
    DateTime ActualOutTime,
    [param: Range(typeof(decimal), "0", "99999999", ErrorMessage = "损坏扣费不能为负数。")]
    decimal Damage,
    [param: StringLength(255, ErrorMessage = "备注不能超过 255 个字符。")]
    string? Note,
    BillingMode Mode);

/// <summary>退房前的账单试算输入。</summary>
public sealed record CheckoutQuoteRequest(
    DateTime ActualOutTime,
    [param: Range(typeof(decimal), "0", "99999999", ErrorMessage = "损坏扣费不能为负数。")]
    decimal Damage,
    BillingMode Mode);
