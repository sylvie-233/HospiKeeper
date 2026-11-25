using HospiKeeper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospiKeeper.Infrastructure.Persistence.Configurations;

/// <summary>追加消费表映射。</summary>
internal sealed class ExtraChargeConfiguration : IEntityTypeConfiguration<ExtraCharge>
{
    public void Configure(EntityTypeBuilder<ExtraCharge> builder)
    {
        builder.ToTable("extra_charge");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.CheckInId).HasColumnName("checkin_id");
        builder.Property(x => x.Item).HasColumnName("item").HasMaxLength(64).IsRequired();
        builder.Property(x => x.Amount).HasColumnName("amount").HasPrecision(10, 2);
        builder.Property(x => x.ChargeTime).HasColumnName("charge_time");
        builder.Property(x => x.OpUserId).HasColumnName("op_user_id");
        builder.Property(x => x.OperatorName).HasColumnName("operator_name").HasMaxLength(64);

        builder.HasIndex(x => x.CheckInId);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.OpUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
