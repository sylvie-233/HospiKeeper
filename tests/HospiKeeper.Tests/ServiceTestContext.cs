using HospiKeeper.Application.Abstractions.Caching;
using HospiKeeper.Application.Caching;
using HospiKeeper.Application.Common;
using HospiKeeper.Application.Services;
using HospiKeeper.Caching;
using HospiKeeper.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HospiKeeper.Tests;

internal sealed class ServiceTestContext : IAsyncDisposable
{
    private ServiceTestContext(
        HotelDbContext db,
        SessionContext session,
        AuthService auth,
        RoomService rooms,
        BookingService booking,
        RecordService records,
        ReportService reports)
    {
        Db = db;
        Session = session;
        Auth = auth;
        Rooms = rooms;
        Booking = booking;
        Records = records;
        Reports = reports;
    }

    public HotelDbContext Db { get; }
    public SessionContext Session { get; }
    public AuthService Auth { get; }
    public RoomService Rooms { get; }
    public BookingService Booking { get; }
    public RecordService Records { get; }
    public ReportService Reports { get; }

    public static async Task<ServiceTestContext> CreateAsync(string username = "admin")
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase($"hospikeeper-tests-{Guid.NewGuid():N}")
            .Options;

        var db = new HotelDbContext(options);
        await DemoDataSeeder.SeedAsync(db, DateTime.Now);

        var session = new SessionContext();
        var clock = new SystemClock();
        var unitOfWork = new EfUnitOfWork(db);
        var logger = new OperationLogger(unitOfWork, session, clock);
        var cacheOptions = Options.Create(new CacheOptions
        {
            Provider = CacheProvider.Memory,
            InstanceName = "hospikeeper-tests:",
            DefaultExpirationSeconds = 60
        });
        var cache = new MemoryCacheService(
            new MemoryCache(new MemoryCacheOptions()),
            cacheOptions,
            NullLogger<MemoryCacheService>.Instance);
        var cacheInvalidator = new HotelCacheInvalidator(cache);
        var auth = new AuthService(unitOfWork, session, logger, clock);
        var login = await auth.LoginAsync(username, "123456");
        if (!login.Success)
        {
            throw new InvalidOperationException(login.Error);
        }

        return new ServiceTestContext(
            db,
            session,
            auth,
            new RoomService(unitOfWork, session, logger, clock, cache, cacheInvalidator),
            new BookingService(unitOfWork, session, logger, clock, cache, cacheInvalidator),
            new RecordService(unitOfWork, session),
            new ReportService(unitOfWork, clock, cache));
    }

    public ValueTask DisposeAsync() => Db.DisposeAsync();
}
