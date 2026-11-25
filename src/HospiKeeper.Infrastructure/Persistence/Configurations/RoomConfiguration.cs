using HospiKeeper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospiKeeper.Infrastructure.Persistence.Configurations;

/// <summary>房间表映射。</summary>
internal sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("room");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.RoomNo).HasColumnName("room_no").HasMaxLength(16).IsRequired();
        builder.Property(x => x.Floor).HasColumnName("floor");
        builder.Property(x => x.TypeId).HasColumnName("type_id");
        builder.Property(x => x.TypeName).HasColumnName("type_name").HasMaxLength(32).IsRequired();
        builder.Property(x => x.Beds).HasColumnName("beds");
        builder.Property(x => x.Price).HasColumnName("price").HasPrecision(10, 2);
        builder.Property(x => x.MinPrice).HasColumnName("min_price").HasPrecision(10, 2);
        builder.Property(x => x.Deposit).HasColumnName("deposit").HasPrecision(10, 2);
        builder.Property(x => x.HasWindow).HasColumnName("has_window");
        builder.Property(x => x.HasBath).HasColumnName("has_bath");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(x => x.Note).HasColumnName("note").HasMaxLength(255);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(x => x.RoomNo).IsUnique();
        builder.HasIndex(x => x.Floor);
        builder.HasIndex(x => x.TypeId);
        builder.HasIndex(x => x.Status);

        builder.HasOne<Domain.Entities.RoomType>()
            .WithMany()
            .HasForeignKey(x => x.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.IsSellable);
    }
}
