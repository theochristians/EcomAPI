using EComAPI.Domain.Transaction.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Transaction.Persistence.Configurations
{
    public class ReturnConfiguration : IEntityTypeConfiguration<Return>
    {
        public void Configure(EntityTypeBuilder<Return> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("Returns");

            entityTypeBuilder.HasKey(r => r.Id);

            entityTypeBuilder.Property(r => r.OrderId).IsRequired();
            entityTypeBuilder.Property(r => r.UserId).IsRequired();

            entityTypeBuilder.Property(r => r.ReturnNumber)
                .IsRequired()
                .HasMaxLength(50);
            entityTypeBuilder.HasIndex(r => r.ReturnNumber).IsUnique();

            entityTypeBuilder.Property(r => r.Reason).IsRequired();

            entityTypeBuilder.Property(r => r.Status)
                .IsRequired()
                .HasMaxLength(50);

            entityTypeBuilder.Property(r => r.RequestedAt).IsRequired();
            entityTypeBuilder.Property(r => r.ApprovedAt).IsRequired(false);
            entityTypeBuilder.Property(r => r.ApprovedBy).IsRequired(false);

            entityTypeBuilder.Property(r => r.RefundAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entityTypeBuilder.Property(r => r.BankName)
                .HasMaxLength(100)
                .IsRequired(false);

            entityTypeBuilder.Property(r => r.BankAccountNumber)
                .HasMaxLength(50)
                .IsRequired(false);

            entityTypeBuilder.Property(r => r.AccountHolderName)
                .HasMaxLength(255)
                .IsRequired(false);

            entityTypeBuilder.Property(r => r.RefundDate).IsRequired(false);

            entityTypeBuilder.HasMany(r => r.Items)
                .WithOne()
                .HasForeignKey(ri => ri.ReturnId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder.HasMany(r => r.Images)
                .WithOne()
                .HasForeignKey(ri => ri.ReturnId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder.HasIndex(r => r.OrderId);
            entityTypeBuilder.HasIndex(r => r.UserId);

            entityTypeBuilder.Property(r => r.CreatedAt).IsRequired();
            entityTypeBuilder.Property(r => r.CreatedBy).IsRequired();
            entityTypeBuilder.Property(r => r.UpdatedAt).IsRequired(false);
            entityTypeBuilder.Property(r => r.UpdatedBy).IsRequired(false);
            entityTypeBuilder.Property(r => r.DeletedAt).IsRequired(false);
            entityTypeBuilder.Property(r => r.DeletedBy).IsRequired(false);
        }
    }
}
