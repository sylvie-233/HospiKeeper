using System.ComponentModel.DataAnnotations;

namespace HospiKeeper.Contracts;

/// <summary>当前登录账号修改密码。</summary>
public sealed record ChangePasswordRequest(
    [param: Required(ErrorMessage = "请输入原密码。")]
    [param: StringLength(100, ErrorMessage = "原密码不能超过 100 个字符。")]
    string OldPassword,
    [param: Required(ErrorMessage = "请输入新密码。")]
    [param: StringLength(100, MinimumLength = 6, ErrorMessage = "新密码长度需为 6 - 100 位。")]
    string NewPassword,
    [param: Required(ErrorMessage = "请再次输入新密码。")]
    [param: StringLength(100, ErrorMessage = "确认密码不能超过 100 个字符。")]
    string ConfirmPassword);
