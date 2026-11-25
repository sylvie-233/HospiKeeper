using HospiKeeper.Domain.Common;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Domain.Services;

/// <summary>一次退房试算结果。</summary>
/// <param name="Mode">计费方式。</param>
/// <param name="Units">计费单位数：按天为晚数，加钟为小时数。</param>
/// <param name="UnitPrice">成交单价。</param>
/// <param name="RoomFee">房费。</param>
/// <param name="ExtraTotal">追加消费合计。</param>
/// <param name="Damage">损坏扣费。</param>
/// <param name="Deposit">已收押金。</param>
/// <param name="Total">应收合计 = 房费 + 追加消费 + 损坏扣费。</param>
/// <param name="Difference">差额 = 应收 − 押金。</param>
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
    /// <summary>按天计费：不足一晚按一晚，返回最少 1 的晚数。</summary>
    public static int CountNights(DateTime checkIn, DateTime checkOut)
    {
        if (checkOut <= checkIn)
        {
            return 0;
        }

        return Math.Max(1, (int)Math.Ceiling((checkOut - checkIn).TotalDays));
    }

    /// <summary>加钟计费：小时数向上取整，返回最少 1 的小时数。</summary>
    public static int CountHours(DateTime checkIn, DateTime checkOut)
    {
        if (checkOut <= checkIn)
        {
            return 0;
        }

        return Math.Max(1, (int)Math.Ceiling((checkOut - checkIn).TotalHours));
    }

    /// <summary>
    /// 计算一次退房账单。加钟模式下优先使用 <paramref name="originPrice"/>（改价前原价）折算，
    /// 避免议价后按小时计费产生歧义。
    /// </summary>
    /// <exception cref="DomainException">金额为负数或退房时间不晚于入住时间。</exception>
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
