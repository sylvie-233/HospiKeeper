using HospiKeeper.Contracts;
using HospiKeeper.Domain.Common;
using HospiKeeper.Domain.Enums;
using Xunit;

namespace HospiKeeper.Tests;

public sealed class BookingWorkflowTests
{
    private static CheckInRequest CreateRequest(
        int roomId,
        decimal price,
        decimal deposit,
        string phone = "13812345678")
    {
        var now = DateTime.Now.AddMinutes(-5);
        return new CheckInRequest(
            roomId,
            "测试客人",
            phone,
            "330101199001011234",
            now,
            now.Date.AddDays(1).AddHours(12),
            price,
            deposit,
            "单元测试",
            Companions: new List<CompanionInput>
            {
                new("同住人", "13900001111", "330101199202021234")
            });
    }

    [Fact]
    public async Task CheckIn_UpdatesRoomAndGeneratesNumber()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        var room = (await ctx.Rooms.GetFreeRoomsAsync()).First();

        var record = await ctx.Booking.CheckInAsync(CreateRequest(room.Id, room.Price, room.Deposit));

        Assert.StartsWith("I", record.CheckInNo);
        Assert.Equal(CheckInStatus.Living, record.Status);
        Assert.Equal(RoomStatus.Occupied, (await ctx.Rooms.GetRoomAsync(room.Id))!.Status);
        Assert.Single(record.Companions);
        Assert.NotNull(await ctx.Booking.GetStayAsync(record.Id));
    }

    [Fact]
    public async Task CheckIn_BelowFloorPrice_IsRejected()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        var room = (await ctx.Rooms.GetFreeRoomsAsync()).First(r => r.MinPrice > 0);

        await Assert.ThrowsAsync<DomainException>(() =>
            ctx.Booking.CheckInAsync(CreateRequest(room.Id, room.MinPrice - 1, room.Deposit)));
    }

    [Fact]
    public async Task CheckIn_InvalidPhone_IsRejected()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        var room = (await ctx.Rooms.GetFreeRoomsAsync()).First();

        await Assert.ThrowsAsync<DomainException>(() =>
            ctx.Booking.CheckInAsync(CreateRequest(room.Id, room.Price, room.Deposit, phone: "123")));
    }

    [Fact]
    public async Task Checkout_GeneratesBillAndMovesRoomToCleaning()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        var room = (await ctx.Rooms.GetFreeRoomsAsync()).First();
        var record = await ctx.Booking.CheckInAsync(CreateRequest(room.Id, room.Price, room.Deposit));

        await ctx.Booking.AddExtraChargeAsync(new ExtraChargeRequest(record.Id, "矿泉水", 4m));
        var refreshed = (await ctx.Booking.GetStayAsync(record.Id))!.CheckIn;
        var actualOut = refreshed.CheckInTime.AddHours(4);
        var quote = ctx.Booking.QuoteCheckout(refreshed, actualOut, 0m, BillingMode.Daily);

        Assert.Equal(room.Price + 4m, quote.Total);
        Assert.Equal(quote.Total - room.Deposit, quote.Difference);

        var bill = await ctx.Booking.CheckoutAsync(new CheckoutRequest(
            record.Id, actualOut, 0m, "退房测试", BillingMode.Daily));

        Assert.StartsWith("S", bill.BillNo);
        Assert.Equal(room.Price + 4m, bill.Total);
        Assert.Equal(quote.Difference, bill.Diff);
        Assert.Equal(RoomStatus.Cleaning, (await ctx.Rooms.GetRoomAsync(room.Id))!.Status);
        Assert.Null(await ctx.Booking.GetStayAsync(record.Id));
    }

    [Fact]
    public async Task CreateAndCancelReservation_ReleasesRoom()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        var room = (await ctx.Rooms.GetFreeRoomsAsync()).First(r => r.RoomNo == "205");

        var reservation = await ctx.Booking.CreateReservationAsync(new ReservationRequest(
            "预定客人", "13700001111", room.Id, DateTime.Now.AddDays(1), null));

        Assert.Equal(RoomStatus.Reserved, (await ctx.Rooms.GetRoomAsync(room.Id))!.Status);
        Assert.Equal(ReservationStatus.Pending, reservation.Status);

        await ctx.Booking.CancelReservationAsync(reservation.Id);

        Assert.Equal(RoomStatus.Free, (await ctx.Rooms.GetRoomAsync(room.Id))!.Status);
        Assert.Equal(ReservationStatus.Cancelled,
            (await ctx.Booking.GetReservationAsync(reservation.Id))!.Status);
    }

    [Fact]
    public async Task ConvertReservation_ToCheckIn()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        var reservation = (await ctx.Booking.GetReservationsAsync(ReservationStatus.Pending)).First();

        var record = await ctx.Booking.CheckInAsync(new CheckInRequest(
            reservation.RoomId,
            reservation.GuestName,
            reservation.GuestPhone,
            "330101199001011234",
            DateTime.Now.AddMinutes(-5),
            DateTime.Today.AddDays(1).AddHours(12),
            168m,
            200m,
            null,
            reservation.Id));

        var updated = (await ctx.Booking.GetReservationAsync(reservation.Id))!;
        Assert.Equal(ReservationStatus.Converted, updated.Status);
        Assert.Equal(record.CheckInNo, updated.CheckInNo);
        Assert.Equal(RoomStatus.Occupied,
            (await ctx.Rooms.GetRoomAsync(reservation.RoomId))!.Status);
    }

    [Fact]
    public async Task CheckIn_InvalidatesRoomCaches()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        var freeBefore = await ctx.Rooms.GetFreeRoomsAsync();
        var room = freeBefore.First();

        await ctx.Booking.CheckInAsync(CreateRequest(room.Id, room.Price, room.Deposit));

        var freeAfter = await ctx.Rooms.GetFreeRoomsAsync();
        Assert.DoesNotContain(freeAfter, r => r.Id == room.Id);
    }

    [Fact]
    public async Task RoomWithHistory_CannotBeDeleted()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        var room = (await ctx.Rooms.GetFreeRoomsAsync()).First();
        var record = await ctx.Booking.CheckInAsync(CreateRequest(room.Id, room.Price, room.Deposit));
        await ctx.Booking.CheckoutAsync(new CheckoutRequest(
            record.Id, record.CheckInTime.AddHours(2), 0m, null, BillingMode.Daily));

        await Assert.ThrowsAsync<DomainException>(() => ctx.Rooms.DeleteRoomAsync(room.Id));
    }

    [Fact]
    public async Task FrontDesk_SeesOnlyOwnRecords()
    {
        await using var ctx = await ServiceTestContext.CreateAsync("front01");
        var records = await ctx.Records.QueryCheckInsAsync(new RecordQuery(null, null, null, null));
        var currentUserId = ctx.Session.CurrentUser!.Id;

        Assert.NotEmpty(records);
        Assert.All(records, r => Assert.Equal(currentUserId, r.OpUserId));
    }

    [Fact]
    public async Task ReportSnapshot_HasMetricsAndRoomTypes()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        var snapshot = await ctx.Reports.GetSnapshotAsync();

        Assert.Equal(30, snapshot.Last30Days.Count);
        Assert.Equal(3, snapshot.RoomTypeStats.Count);
        Assert.True(snapshot.SellableRooms > 0);
        Assert.True(snapshot.TodayRevenue >= 0);
        Assert.True(snapshot.TodayCheckIns >= 0);
    }

    [Fact]
    public async Task RevenueRange_ComputesPreviousPeriodComparison()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        var end = DateTime.Today;
        var result = await ctx.Reports.GetRevenueRangeAsync(end.AddDays(-6), end);

        Assert.Equal(7, result.Days.Count);
        Assert.Equal(7, result.Rows.Count);
        Assert.True(result.DailyAverage >= 0);
    }
}
