using EComAPI.Domain.Auth.Entities;
using EComAPI.Infrastructure.Common.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Auth.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(200);

            builder.OwnsOne(x => x.Email, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(255);

                email.HasIndex(e => e.Value).IsUnique();
            });

            builder.OwnsOne(x => x.Password, pwd =>
            {
                pwd.Property(p => p.Value)
                    .HasColumnName("PasswordHash")
                    .IsRequired();
            });

            builder.Property(x => x.RoleId).IsRequired();
            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.IsEmailVerified).IsRequired();

            builder.ConfigureAudit();
        }
    }
}