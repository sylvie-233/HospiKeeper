using HospiKeeper.Application.Abstractions.Services;
using HospiKeeper.Application.Caching;
using HospiKeeper.Application.Common;
using HospiKeeper.Application.Services;
using Sylvie233.Caching;
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
        ReportService reports,
        GuestBookingService guest,
        SiteSettingsService siteSettings)
    {
        Db = db;
        Session = session;
        Auth = auth;
        Rooms = rooms;
        Booking = booking;
        Records = records;
        Reports = reports;
        Guest = guest;
        SiteSettings = siteSettings;
    }

    public HotelDbContext Db { get; }
    public SessionContext Session { get; }
    public AuthService Auth { get; }
    public RoomService Rooms { get; }
    public BookingService Booking { get; }
    public RecordService Records { get; }
    public ReportService Reports { get; }
    public GuestBookingService Guest { get; }
    public SiteSettingsService SiteSettings { get; }

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
            new ReportService(unitOfWork, clock, cache),
            new GuestBookingService(unitOfWork, clock, cache, cacheInvalidator, logger),
            new SiteSettingsService(cache, clock, session));
    }

    public ValueTask DisposeAsync() => Db.DisposeAsync();
}
