using System.ComponentModel.DataAnnotations;
using HospiKeeper.Contracts;

namespace HospiKeeper.Web.Services;

/// <summary>预订弹窗表单模型，校验规则与 API 契约保持一致。</summary>
public sealed class BookingFormModel
{
    [Required(ErrorMessage = "请输入入住人姓名。")]
    [StringLength(20, ErrorMessage = "入住人姓名不能超过 20 个字符。")]
    public string GuestName { get; set; } = string.Empty;

    [Required(ErrorMessage = "请输入手机号。")]
    [RegularExpression(ValidationPatterns.Mobile, ErrorMessage = "请输入 11 位有效手机号。")]
    public string GuestPhone { get; set; } = string.Empty;

    [RegularExpression(ValidationPatterns.OptionalIdCard, ErrorMessage = "身份证号格式无效。")]
    public string? GuestIdCard { get; set; }

    [StringLength(255, ErrorMessage = "备注不能超过 255 个字符。")]
    public string? Note { get; set; }
}
