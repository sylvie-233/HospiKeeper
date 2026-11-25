using HospiKeeper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospiKeeper.Infrastructure.Persistence.Configurations;

/// <summary>操作员账号表映射。</summary>
internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("sys_user");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.Username).HasColumnName("username").HasMaxLength(32).IsRequired();
        builder.Property(x => x.PasswordHash).HasColumnName("password").HasMaxLength(64).IsRequired();
        builder.Property(x => x.RealName).HasColumnName("real_name").HasMaxLength(32).IsRequired();
        builder.Property(x => x.Role).HasColumnName("role").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(x => x.LastLogin).HasColumnName("last_login");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(x => x.Username).IsUnique();

        builder.Ignore(x => x.IsAdmin);
        builder.Ignore(x => x.RoleDisplay);
        builder.Ignore(x => x.DisplayName);
    }
}
