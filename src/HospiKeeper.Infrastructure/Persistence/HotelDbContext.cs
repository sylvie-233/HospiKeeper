using HospiKeeper.Domain.Entities;
using HospiKeeper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospiKeeper.Infrastructure.Persistence;

public sealed class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<RoomType> RoomTypes => Set<RoomType>();

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<Reservation> Reservations => Set<Reservation>();

    public DbSet<CheckInRecord> CheckIns => Set<CheckInRecord>();

    public DbSet<Companion> Companions => Set<Companion>();

    public DbSet<ExtraCharge> ExtraCharges => Set<ExtraCharge>();

    public DbSet<CheckoutBill> Bills => Set<CheckoutBill>();

    public DbSet<BillLine> BillLines => Set<BillLine>();

    public DbSet<OperationLog> OperationLogs => Set<OperationLog>();

    public DbSet<DocumentSequence> DocumentSequences => Set<DocumentSequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);
    }
}
