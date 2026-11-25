using HospiKeeper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospiKeeper.Infrastructure.Persistence.Configurations;

/// <summary>账单消费明细表映射。</summary>
internal sealed class BillLineConfiguration : IEntityTypeConfiguration<BillLine>
{
    public void Configure(EntityTypeBuilder<BillLine> builder)
    {
        builder.ToTable("checkout_bill_line");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.CheckoutBillId).HasColumnName("checkout_bill_id");
        builder.Property(x => x.Item).HasColumnName("item").HasMaxLength(64).IsRequired();
        builder.Property(x => x.Amount).HasColumnName("amount").HasPrecision(10, 2);

        builder.HasIndex(x => x.CheckoutBillId);
    }
}
