using HospiKeeper.Domain.Entities;
using HospiKeeper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospiKeeper.Infrastructure.Persistence;

/// <summary>酒店业务 DbContext：聚合全部实体集合，映射由 Configurations 下的配置类定义。</summary>
public sealed class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
    {
    }

    /// <summary>操作员账号。</summary>
    public DbSet<AppUser> Users => Set<AppUser>();

    /// <summary>房型。</summary>
    public DbSet<RoomType> RoomTypes => Set<RoomType>();

    /// <summary>房间。</summary>
    public DbSet<Room> Rooms => Set<Room>();

    /// <summary>预定。</summary>
    public DbSet<Reservation> Reservations => Set<Reservation>();

    /// <summary>入住记录。</summary>
    public DbSet<CheckInRecord> CheckIns => Set<CheckInRecord>();

    public DbSet<Companion> Companions => Set<Companion>();

    public DbSet<ExtraCharge> ExtraCharges => Set<ExtraCharge>();

    /// <summary>结算账单。</summary>
    public DbSet<CheckoutBill> Bills => Set<CheckoutBill>();

    public DbSet<BillLine> BillLines => Set<BillLine>();

    public DbSet<OperationLog> OperationLogs => Set<OperationLog>();

    /// <summary>业务单号序列。</summary>
    public DbSet<DocumentSequence> DocumentSequences => Set<DocumentSequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);
    }
}
