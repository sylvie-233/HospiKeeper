using HospiKeeper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospiKeeper.Infrastructure.Persistence.Configurations;

/// <summary>操作日志表映射。</summary>
internal sealed class OperationLogConfiguration : IEntityTypeConfiguration<OperationLog>
{
    public void Configure(EntityTypeBuilder<OperationLog> builder)
    {
        builder.ToTable("op_log");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.UserName).HasColumnName("user_name").HasMaxLength(64);
        builder.Property(x => x.Action).HasColumnName("action").HasMaxLength(32).IsRequired();
        builder.Property(x => x.Target).HasColumnName("target").HasMaxLength(64);
        builder.Property(x => x.Detail).HasColumnName("detail").HasMaxLength(500);
        builder.Property(x => x.BizNo).HasColumnName("biz_no").HasMaxLength(32);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Action);
        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
