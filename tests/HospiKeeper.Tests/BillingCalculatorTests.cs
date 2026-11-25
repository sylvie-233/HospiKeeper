using HospiKeeper.Domain.Common;
using HospiKeeper.Domain.Enums;
using HospiKeeper.Domain.Services;
using Xunit;

namespace HospiKeeper.Tests;

/// <summary>计费口径的单元测试。</summary>
public sealed class BillingCalculatorTests
{
    [Fact]
    public void CountNights_AcrossNoon_IsOneNight()
    {
        var nights = BillingCalculator.CountNights(
            new DateTime(2026, 11, 25, 14, 0, 0),
            new DateTime(2026, 11, 26, 12, 0, 0));

        Assert.Equal(1, nights);
    }

    [Fact]
    public void CountNights_FullTwoDays_IsTwoNights()
    {
        var nights = BillingCalculator.CountNights(
            new DateTime(2026, 11, 23, 14, 0, 0),
            new DateTime(2026, 11, 25, 14, 0, 0));

        Assert.Equal(2, nights);
    }

    [Fact]
    public void CountNights_ShortStayStillCountsOneNight()
    {
        var nights = BillingCalculator.CountNights(
            new DateTime(2026, 11, 25, 14, 0, 0),
            new DateTime(2026, 11, 25, 18, 0, 0));

        Assert.Equal(1, nights);
    }

    [Fact]
    public void Calculate_Daily_ProducesExpectedTotalAndDifference()
    {
        var result = BillingCalculator.Calculate(
            new DateTime(2026, 11, 25, 14, 0, 0),
            new DateTime(2026, 11, 26, 12, 0, 0),
            price: 198m,
            deposit: 200m,
            extraTotal: 6m,
            damage: 10m,
            BillingMode.Daily);

        Assert.Equal(1, result.Units);
        Assert.Equal(198m, result.RoomFee);
        Assert.Equal(214m, result.Total);
        Assert.Equal(14m, result.Difference);
        Assert.True(result.NeedsPayment);
    }

    [Fact]
    public void Calculate_Hourly_RoundsUpHourlyFee()
    {
        var result = BillingCalculator.Calculate(
            new DateTime(2026, 11, 25, 14, 0, 0),
            new DateTime(2026, 11, 25, 17, 0, 0),
            price: 198m,
            deposit: 100m,
            extraTotal: 0m,
            damage: 0m,
            BillingMode.Hourly);

        Assert.Equal(3, result.Units);
        Assert.Equal(25m, result.RoomFee); // ceil(3 * 198 / 24)
        Assert.Equal(-75m, result.Difference);
        Assert.True(result.NeedsRefund);
    }

    [Fact]
    public void Calculate_InvalidCheckoutTime_Throws()
    {
        Assert.Throws<DomainException>(() => BillingCalculator.Calculate(
            new DateTime(2026, 11, 25, 14, 0, 0),
            new DateTime(2026, 11, 25, 13, 0, 0),
            198m, 100m, 0m, 0m, BillingMode.Daily));
    }
}
