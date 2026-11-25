using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospiKeeper.Infrastructure.Persistence.Configurations;

/// <summary>预定表映射。</summary>
internal sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservation");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.ResNo).HasColumnName("res_no").HasMaxLength(32).IsRequired();
        builder.Property(x => x.GuestName).HasColumnName("guest_name").HasMaxLength(32).IsRequired();
        builder.Property(x => x.GuestPhone).HasColumnName("guest_phone").HasMaxLength(20).IsRequired();
        builder.Property(x => x.GuestIdCard).HasColumnName("guest_id_card").HasMaxLength(18);
        builder.Property(x => x.RoomId).HasColumnName("room_id");
        builder.Property(x => x.RoomNo).HasColumnName("room_no").HasMaxLength(16);
        builder.Property(x => x.TypeId).HasColumnName("type_id").IsRequired();
        builder.Property(x => x.CheckInDate).HasColumnName("check_in_date").HasColumnType("date").IsRequired();
        builder.Property(x => x.CheckOutDate).HasColumnName("check_out_date").HasColumnType("date").IsRequired();
        builder.Property(x => x.Channel)
            .HasColumnName("channel")
            .HasConversion(
                value => value == ReservationChannel.Web ? "WEB" : "WALK_IN",
                value => value == "WEB" ? ReservationChannel.Web : ReservationChannel.WalkIn)
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnName("unit_price").HasPrecision(10, 2);
        builder.Property(x => x.RoomTypeName).HasColumnName("room_type_name").HasMaxLength(32).IsRequired();
        builder.Property(x => x.ArriveTime).HasColumnName("arrive_time");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(x => x.CheckInId).HasColumnName("checkin_id");
        builder.Property(x => x.CheckInNo).HasColumnName("checkin_no").HasMaxLength(32);
        builder.Property(x => x.Note).HasColumnName("note").HasMaxLength(255);
        builder.Property(x => x.OpUserId).HasColumnName("op_user_id");
        builder.Property(x => x.OperatorName).HasColumnName("operator_name").HasMaxLength(64);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(x => x.ResNo).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ArriveTime);
        builder.HasIndex(x => x.GuestPhone);
        builder.HasIndex(x => new { x.TypeId, x.CheckInDate, x.CheckOutDate });

        builder.HasOne<Room>()
            .WithMany()
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<RoomType>()
            .WithMany()
            .HasForeignKey(x => x.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CheckIn)
            .WithMany()
            .HasForeignKey(x => x.CheckInId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.OpUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
