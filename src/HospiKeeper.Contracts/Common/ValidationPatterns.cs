namespace HospiKeeper.Contracts;

/// <summary>
/// 契约层可复用的校验正则。带 <c>Optional</c> 的版本允许空字符串，
/// 用于同住人等可选字段；业务层的 <c>InputGuard</c> 仍会做最终校验。
/// </summary>
public static class ValidationPatterns
{
    /// <summary>中国大陆 11 位手机号。</summary>
    public const string Mobile = @"^1[3-9]\d{9}$";

    /// <summary>可为空的手机号。</summary>
    public const string OptionalMobile = @"^$|^1[3-9]\d{9}$";

    /// <summary>18 位身份证号，末位可为 X。</summary>
    public const string IdCard = @"^\d{17}[\dXx]$";

    /// <summary>可为空的身份证号。</summary>
    public const string OptionalIdCard = @"^$|^\d{17}[\dXx]$";
}
