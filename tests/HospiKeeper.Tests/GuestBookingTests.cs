using HospiKeeper.Application.Common;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Enums;
using Xunit;

namespace HospiKeeper.Tests;

/// <summary>游客在线预订：房型库存、免锁房下单、取消与转入住衔接。</summary>
public sealed class GuestBookingTests
{
    [Fact]
    public async Task Availability_ReturnsSellableMinusOverlappingPending()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        var result = await ctx.Guest.GetAvailabilityAsync(DateTime.Today, DateTime.Today.AddDays(1));

        Assert.Equal(3, result.RoomTypes.Count);
        Assert.Equal(1, result.Nights);
        Assert.All(result.RoomTypes, type => Assert.True(type.Remaining >= 0));
        Assert.All(result.RoomTypes, type => Assert.True(type.SellableRooms >= type.Remaining));
    }

    [Fact]
    public async Task Create_DoesNotLockRoom_AndCancelRestoresStock()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        var availability = await ctx.Guest.GetAvailabilityAsync(DateTime.Today, DateTime.Today.AddDays(1));
        var target = availability.RoomTypes.First(t => t.Remaining > 0);
        var reservedBefore = ctx.Db.Rooms.Count(r => r.Status == RoomStatus.Reserved);

        var created = await ctx.Guest.CreateAsync(new GuestBookingRequest(
            target.TypeId,
            DateTime.Today,
            DateTime.Today.AddDays(1),
            "游客甲",
            "13800138001",
            "330101199001011234",
            "测试预订"));

        Assert.Equal(ReservationStatus.Pending, created.Status);
        Assert.Null(created.RoomNo);
        Assert.Equal(ReservationChannel.Web, created.Channel);
        Assert.Equal(target.BasePrice, created.UnitPrice);
        Assert.Equal(target.BasePrice, created.Total);
        Assert.Equal(reservedBefore, ctx.Db.Rooms.Count(r => r.Status == RoomStatus.Reserved));

        var afterCreate = await ctx.Guest.GetAvailabilityAsync(DateTime.Today, DateTime.Today.AddDays(1));
        Assert.Equal(target.Remaining - 1, afterCreate.RoomTypes.First(t => t.TypeId == target.TypeId).Remaining);

        var mine = await ctx.Guest.GetByPhoneAsync("13800138001");
        Assert.Single(mine);
        Assert.Equal(created.ResNo, mine[0].ResNo);

        await ctx.Guest.CancelAsync(created.ResNo, "13800138001");
        var afterCancel = await ctx.Guest.GetAvailabilityAsync(DateTime.Today, DateTime.Today.AddDays(1));
        Assert.Equal(target.Remaining, afterCancel.RoomTypes.First(t => t.TypeId == target.TypeId).Remaining);
    }

    [Fact]
    public async Task ConvertWebReservation_AssignsRoomAndBackfillsReservation()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        var availability = await ctx.Guest.GetAvailabilityAsync(DateTime.Today, DateTime.Today.AddDays(1));
        var target = availability.RoomTypes.First(t => t.TypeId == 2 && t.Remaining > 0);

        var created = await ctx.Guest.CreateAsync(new GuestBookingRequest(
            target.TypeId,
            DateTime.Today,
            DateTime.Today.AddDays(1),
            "游客乙",
            "13800138002",
            null,
            null));

        var room = (await ctx.Rooms.GetRoomsAsync(status: RoomStatus.Free))
            .First(r => r.TypeId == target.TypeId);

        var stay = await ctx.Booking.CheckInAsync(new CheckInRequest(
            room.Id,
            created.GuestName,
            created.GuestPhone,
            "330101199001011234",
            DateTime.Now,
            DateTime.Today.AddDays(1).AddHours(12),
            room.Price,
            room.Deposit,
            null,
            ReservationId: ctx.Db.Reservations.Single(r => r.ResNo == created.ResNo).Id));

        var reservation = ctx.Db.Reservations.Single(r => r.ResNo == created.ResNo);
        Assert.Equal(ReservationStatus.Converted, reservation.Status);
        Assert.Equal(room.Id, reservation.RoomId);
        Assert.Equal(room.RoomNo, reservation.RoomNo);
        Assert.Equal(stay.CheckInNo, reservation.CheckInNo);
        Assert.Equal(RoomStatus.Occupied, (await ctx.Rooms.GetRoomAsync(room.Id))!.Status);
    }

    [Fact]
    public async Task Create_RejectsInvalidDateRange()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        await Assert.ThrowsAsync<HospiKeeper.Domain.Common.DomainException>(() =>
            ctx.Guest.CreateAsync(new GuestBookingRequest(
                1,
                DateTime.Today,
                DateTime.Today,
                "游客丙",
                "13800138003",
                null,
                null)));
    }
}
