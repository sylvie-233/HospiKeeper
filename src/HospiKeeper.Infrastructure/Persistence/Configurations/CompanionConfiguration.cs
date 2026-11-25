using HospiKeeper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospiKeeper.Infrastructure.Persistence.Configurations;

internal sealed class CompanionConfiguration : IEntityTypeConfiguration<Companion>
{
    public void Configure(EntityTypeBuilder<Companion> builder)
    {
        builder.ToTable("checkin_companion");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.CheckInId).HasColumnName("checkin_id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(32).IsRequired();
        builder.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(x => x.IdCard).HasColumnName("idcard").HasMaxLength(18);

        builder.HasIndex(x => x.CheckInId);
    }
}
