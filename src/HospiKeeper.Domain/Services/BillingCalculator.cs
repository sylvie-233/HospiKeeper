using HospiKeeper.Domain.Common;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Domain.Services;

/// <summary>一次退房试算结果。</summary>
public sealed record BillingResult(
    BillingMode Mode,
    int Units,
    decimal UnitPrice,
    decimal RoomFee,
    decimal ExtraTotal,
    decimal Damage,
    decimal Deposit,
    decimal Total,
    decimal Difference)
{
    /// <summary>应补收。</summary>
    public bool NeedsPayment => Difference > 0m;

    /// <summary>应退款。</summary>
    public bool NeedsRefund => Difference < 0m;
}

/// <summary>
/// 计费口径（与原型一致）：
/// · 按天：不足一晚按一晚计，跨 12:00 视为新一天；
/// · 加钟：按小时向上取整，房费 = ceil(小时数 × 标价 ÷ 24)；
/// · 应收 = 房费 + 追加消费 + 损坏扣费；
/// · 差额 = 应收 − 已收押金。
/// </summary>
public static class BillingCalculator
{
    public static int CountNights(DateTime checkIn, DateTime checkOut)
    {
        if (checkOut <= checkIn)
        {
            return 0;
        }

        return Math.Max(1, (int)Math.Ceiling((checkOut - checkIn).TotalDays));
    }

    public static int CountHours(DateTime checkIn, DateTime checkOut)
    {
        if (checkOut <= checkIn)
        {
            return 0;
        }

        return Math.Max(1, (int)Math.Ceiling((checkOut - checkIn).TotalHours));
    }

    public static BillingResult Calculate(
        DateTime checkIn,
        DateTime checkOut,
        decimal price,
        decimal deposit,
        decimal extraTotal,
        decimal damage,
        BillingMode mode,
        decimal? originPrice = null)
    {
        if (price < 0m || deposit < 0m || extraTotal < 0m || damage < 0m)
        {
            throw new DomainException("金额不能为负数。");
        }

        if (checkOut <= checkIn)
        {
            throw new DomainException("实际退房时间必须晚于入住时间。");
        }

        int units;
        decimal roomFee;

        if (mode == BillingMode.Hourly)
        {
            units = CountHours(checkIn, checkOut);
            var hourlyPrice = originPrice is > 0m ? originPrice.Value : price;
            roomFee = Math.Ceiling(units * hourlyPrice / 24m);
        }
        else
        {
            units = CountNights(checkIn, checkOut);
            roomFee = units * price;
        }

        var total = roomFee + extraTotal + damage;
        var difference = total - deposit;

        return new BillingResult(mode, units, price, roomFee, extraTotal, damage, deposit, total, difference);
    }
}
