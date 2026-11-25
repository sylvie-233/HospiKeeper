using System.ComponentModel.DataAnnotations;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Contracts;

/// <summary>创建 / 修改操作员账号的输入。</summary>
/// <param name="Username">登录账号，创建后不建议修改。</param>
/// <param name="RealName">真实姓名。</param>
/// <param name="Role">角色。</param>
/// <param name="Status">账号状态。</param>
/// <param name="Password">初始密码；修改时可留空表示不重置。</param>
public sealed record AccountInput(
    [param: Required(ErrorMessage = "请输入登录账号。")]
    [param: StringLength(32, ErrorMessage = "登录账号不能超过 32 个字符。")]
    string Username,
    [param: Required(ErrorMessage = "请输入姓名。")]
    [param: StringLength(32, ErrorMessage = "姓名不能超过 32 个字符。")]
    string RealName,
    UserRole Role,
    AccountStatus Status,
    [param: StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度需为 6 - 100 位。")]
    string? Password);

/// <summary>启用 / 停用账号。</summary>
/// <param name="Status">目标状态。</param>
public sealed record AccountStatusRequest(AccountStatus Status);
