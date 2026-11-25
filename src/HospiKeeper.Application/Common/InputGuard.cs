using System.Text.RegularExpressions;
using HospiKeeper.Domain.Common;

namespace HospiKeeper.Application.Common;

/// <summary>统一输入校验，异常消息可直接展示给操作员。</summary>
public static partial class InputGuard
{
    [GeneratedRegex(@"^1[3-9]\d{9}$")]
    private static partial Regex MobileRegex();

    [GeneratedRegex(@"^\d{17}[\dXx]$")]
    private static partial Regex IdCardRegex();

    public static string Text(string? value, string field, int maxLength = 64)
    {
        var text = value?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(text))
        {
            throw new DomainException($"请输入{field}。");
        }

        if (text.Length > maxLength)
        {
            throw new DomainException($"{field}不能超过 {maxLength} 个字符。");
        }

        return text;
    }

    public static string? OptionalText(string? value, string field, int maxLength = 255)
    {
        var text = value?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        if (text.Length > maxLength)
        {
            throw new DomainException($"{field}不能超过 {maxLength} 个字符。");
        }

        return text;
    }

    public static string Phone(string? value)
    {
        var text = value?.Trim() ?? string.Empty;
        if (!MobileRegex().IsMatch(text))
        {
            throw new DomainException("请输入 11 位有效手机号。");
        }

        return text;
    }

    public static string? OptionalPhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Phone(value);
    }

    public static string IdCard(string? value)
    {
        var text = value?.Trim() ?? string.Empty;
        if (!IdCardRegex().IsMatch(text))
        {
            throw new DomainException("请输入 18 位有效身份证号（末位可为 X）。");
        }

        return text.ToUpperInvariant();
    }

    public static string? OptionalIdCard(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return IdCard(value);
    }

    public static decimal Money(decimal value, string field, bool allowZero = true)
    {
        if (value < 0m || (!allowZero && value == 0m))
        {
            throw new DomainException(allowZero ? $"{field}不能为负数。" : $"{field}必须大于 0。");
        }

        return decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    public static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new DomainException(message);
        }
    }

    public static DateTime ParseDateTime(string? text, string field)
    {
        if (!DateTime.TryParse(text, out var value))
        {
            throw new DomainException($"{field}格式无效，请使用 yyyy-MM-dd HH:mm。");
        }

        return value;
    }

    public static string FormatDateTime(DateTime value) => value.ToString("yyyy-MM-dd HH:mm");
}
