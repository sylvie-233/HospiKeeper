using HospiKeeper.Contracts;
using HospiKeeper.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HospiKeeper.Tests;

/// <summary>房型改价应同步房间冗余标价并写入改价日志。</summary>
public sealed class RoomTypeTests
{
    [Fact]
    public async Task UpdateRoomType_SyncsRoomPriceAndWritesLog()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        var type = await ctx.Db.RoomTypes.OrderBy(t => t.Id).FirstAsync();
        var rooms = await ctx.Db.Rooms.Where(r => r.TypeId == type.Id).ToListAsync();
        var newPrice = type.BasePrice + 20m;

        await ctx.Rooms.UpdateRoomTypeAsync(
            type.Id,
            new RoomTypeInput(type.Name, newPrice, type.Note));

        var updated = await ctx.Db.Rooms.Where(r => r.TypeId == type.Id).ToListAsync();
        Assert.NotEmpty(updated);
        Assert.All(updated, room => Assert.Equal(newPrice, room.Price));

        var log = await ctx.Db.OperationLogs
            .OrderByDescending(l => l.Id)
            .FirstAsync(l => l.Action == OperationActions.ModifyPrice);
        Assert.Contains("已同步房间标价", log.Detail);
    }
}
