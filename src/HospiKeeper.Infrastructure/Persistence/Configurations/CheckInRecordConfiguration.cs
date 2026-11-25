using HospiKeeper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospiKeeper.Infrastructure.Persistence.Configurations;

/// <summary>入住记录表映射。</summary>
internal sealed class CheckInRecordConfiguration : IEntityTypeConfiguration<CheckInRecord>
{
    public void Configure(EntityTypeBuilder<CheckInRecord> builder)
    {
        builder.ToTable("checkin_record");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.CheckInNo).HasColumnName("checkin_no").HasMaxLength(32).IsRequired();
        builder.Property(x => x.RoomId).HasColumnName("room_id");
        builder.Property(x => x.RoomNo).HasColumnName("room_no").HasMaxLength(16).IsRequired();
        builder.Property(x => x.RoomTypeName).HasColumnName("room_type_name").HasMaxLength(32).IsRequired();
        builder.Property(x => x.GuestName).HasColumnName("guest_name").HasMaxLength(32).IsRequired();
        builder.Property(x => x.GuestPhone).HasColumnName("guest_phone").HasMaxLength(20).IsRequired();
        builder.Property(x => x.GuestIdCard).HasColumnName("guest_idcard").HasMaxLength(18).IsRequired();
        builder.Property(x => x.CheckInTime).HasColumnName("checkin_time");
        builder.Property(x => x.PlanOutTime).HasColumnName("plan_out_time");
        builder.Property(x => x.ActualOutTime).HasColumnName("actual_out_time");
        builder.Property(x => x.Price).HasColumnName("price").HasPrecision(10, 2);
        builder.Property(x => x.OriginPrice).HasColumnName("origin_price").HasPrecision(10, 2);
        builder.Property(x => x.Deposit).HasColumnName("deposit").HasPrecision(10, 2);
        builder.Property(x => x.ExtendHours).HasColumnName("extend_hours");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(x => x.Note).HasColumnName("note").HasMaxLength(255);
        builder.Property(x => x.OpUserId).HasColumnName("op_user_id");
        builder.Property(x => x.OperatorName).HasColumnName("operator_name").HasMaxLength(64);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(x => x.CheckInNo).IsUnique();
        builder.HasIndex(x => x.RoomNo);
        builder.HasIndex(x => x.GuestName);
        builder.HasIndex(x => x.GuestPhone);
        builder.HasIndex(x => x.GuestIdCard);
        builder.HasIndex(x => x.CheckInTime);
        builder.HasIndex(x => x.Status);

        builder.HasOne<Room>()
            .WithMany()
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.OpUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Companions)
            .WithOne()
            .HasForeignKey(x => x.CheckInId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ExtraCharges)
            .WithOne()
            .HasForeignKey(x => x.CheckInId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.ExtraTotal);
    }
}
