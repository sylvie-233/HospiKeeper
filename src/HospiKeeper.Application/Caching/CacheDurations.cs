namespace HospiKeeper.Application.Caching;

/// <summary>按数据实时性要求集中定义缓存时长。</summary>
public static class CacheDurations
{
    public static readonly TimeSpan Board = TimeSpan.FromSeconds(15);
    public static readonly TimeSpan Counts = TimeSpan.FromSeconds(15);
    public static readonly TimeSpan Stays = TimeSpan.FromSeconds(15);
    public static readonly TimeSpan Reservations = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan Rooms = TimeSpan.FromSeconds(60);
    public static readonly TimeSpan RoomTypes = TimeSpan.FromSeconds(60);
    public static readonly TimeSpan Reports = TimeSpan.FromSeconds(60);
}
