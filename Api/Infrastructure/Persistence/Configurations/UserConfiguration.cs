using Api.Domain.Entities;
using Api.Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder
            .Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(1000);
        
        builder
            .Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);
        
        builder
            .Property(u => u.Password)
            .IsRequired()
            .HasMaxLength(255);

        builder
            .Property(u => u.Role)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<Role>(v, true)
            )
            .HasMaxLength(10);
    }
}
