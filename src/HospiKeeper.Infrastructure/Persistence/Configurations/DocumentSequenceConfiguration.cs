using HospiKeeper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospiKeeper.Infrastructure.Persistence.Configurations;

internal sealed class DocumentSequenceConfiguration : IEntityTypeConfiguration<DocumentSequence>
{
    public void Configure(EntityTypeBuilder<DocumentSequence> builder)
    {
        builder.ToTable("doc_sequence");
        builder.HasKey(x => new { x.Prefix, x.DocDate });

        builder.Property(x => x.Prefix).HasColumnName("prefix").HasMaxLength(8).IsRequired();
        builder.Property(x => x.DocDate).HasColumnName("doc_date").HasColumnType("date");
        builder.Property(x => x.Sequence).HasColumnName("seq").IsRequired();
    }
}
