using HospiKeeper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospiKeeper.Infrastructure.Persistence.Configurations;

internal sealed class CheckoutBillConfiguration : IEntityTypeConfiguration<CheckoutBill>
{
    public void Configure(EntityTypeBuilder<CheckoutBill> builder)
    {
        builder.ToTable("checkout_bill");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.BillNo).HasColumnName("bill_no").HasMaxLength(32).IsRequired();
        builder.Property(x => x.CheckInId).HasColumnName("checkin_id");
        builder.Property(x => x.CheckInNo).HasColumnName("checkin_no").HasMaxLength(32);
        builder.Property(x => x.RoomId).HasColumnName("room_id");
        builder.Property(x => x.RoomNo).HasColumnName("room_no").HasMaxLength(16).IsRequired();
        builder.Property(x => x.RoomTypeName).HasColumnName("room_type_name").HasMaxLength(32);
        builder.Property(x => x.GuestName).HasColumnName("guest_name").HasMaxLength(32).IsRequired();
        builder.Property(x => x.CheckInTime).HasColumnName("checkin_time");
        builder.Property(x => x.CheckoutTime).HasColumnName("checkout_time");
        builder.Property(x => x.Mode).HasColumnName("billing_mode").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(x => x.Nights).HasColumnName("nights");
        builder.Property(x => x.RoomFee).HasColumnName("room_fee").HasPrecision(10, 2);
        builder.Property(x => x.ExtraTotal).HasColumnName("extra_total").HasPrecision(10, 2);
        builder.Property(x => x.Damage).HasColumnName("damage").HasPrecision(10, 2);
        builder.Property(x => x.OtherFee).HasColumnName("other_fee").HasPrecision(10, 2);
        builder.Property(x => x.DepositReceived).HasColumnName("deposit_received").HasPrecision(10, 2);
        builder.Property(x => x.DepositUsed).HasColumnName("deposit_used").HasPrecision(10, 2);
        builder.Property(x => x.Total).HasColumnName("total").HasPrecision(10, 2);
        builder.Property(x => x.Diff).HasColumnName("diff").HasPrecision(10, 2);
        builder.Property(x => x.Note).HasColumnName("note").HasMaxLength(255);
        builder.Property(x => x.OpUserId).HasColumnName("op_user_id");
        builder.Property(x => x.OperatorName).HasColumnName("operator_name").HasMaxLength(64);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(x => x.BillNo).IsUnique();
        builder.HasIndex(x => x.CheckInId).IsUnique();
        builder.HasIndex(x => x.RoomNo);
        builder.HasIndex(x => x.GuestName);
        builder.HasIndex(x => x.CheckoutTime);

        builder.HasOne<CheckInRecord>()
            .WithMany()
            .HasForeignKey(x => x.CheckInId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Room>()
            .WithMany()
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.OpUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Extras)
            .WithOne()
            .HasForeignKey(x => x.CheckoutBillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
