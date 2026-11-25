namespace HospiKeeper.Contracts;

/// <summary>看板上的一个房间及其当前住客 / 预定信息。</summary>
/// <param name="Room">房间。</param>
/// <param name="ActiveStay">当前在住记录，空闲时为 <c>null</c>。</param>
/// <param name="PendingReservation">待入住预定，无预定时为 <c>null</c>。</param>
public sealed record BoardRoom(RoomDto Room, CheckInRecordDto? ActiveStay, ReservationDto? PendingReservation);

/// <summary>在住列表的一行。</summary>
/// <param name="CheckIn">入住记录。</param>
/// <param name="ExtraTotal">追加消费合计。</param>
/// <param name="EstimatedRoomFee">按当前时间估算的房费。</param>
public sealed record StayInfo(CheckInRecordDto CheckIn, decimal ExtraTotal, decimal EstimatedRoomFee)
{
    /// <summary>预计应收 = 房费 + 追加消费。</summary>
    public decimal EstimatedTotal => EstimatedRoomFee + ExtraTotal;
}

/// <summary>前台房态汇总计数。</summary>
/// <param name="Living">在住房间数。</param>
/// <param name="PendingReservations">待入住预定数。</param>
/// <param name="Cleaning">打扫中房间数。</param>
public sealed record FrontDeskCounts(int Living, int PendingReservations, int Cleaning);
