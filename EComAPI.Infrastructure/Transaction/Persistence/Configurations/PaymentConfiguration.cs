using EComAPI.Domain.Transaction.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Transaction.Persistence.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("Payments");

            entityTypeBuilder.HasKey(payment => payment.Id);

            entityTypeBuilder.Property(payment => payment.OrderId).IsRequired();
            entityTypeBuilder.HasIndex(payment => payment.OrderId).IsUnique();

            entityTypeBuilder.Property(payment => payment.PaymentMethod)
                .HasMaxLength(50)
                .IsRequired(false);

            entityTypeBuilder.Property(payment => payment.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entityTypeBuilder.Property(payment => payment.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("pending");

            entityTypeBuilder.Property(payment => payment.ProofImageUrl)
                .HasMaxLength(500)
                .IsRequired(false);

            entityTypeBuilder.Property(payment => payment.AdminNote)
                .HasMaxLength(500)
                .IsRequired(false);

            entityTypeBuilder.Property(payment => payment.ConfirmedAt).IsRequired(false);
            entityTypeBuilder.Property(payment => payment.ConfirmedBy).IsRequired(false);

            entityTypeBuilder.Property(payment => payment.CreatedAt).IsRequired();
            entityTypeBuilder.Property(payment => payment.CreatedBy).IsRequired();
            entityTypeBuilder.Property(payment => payment.UpdatedAt).IsRequired(false);
            entityTypeBuilder.Property(payment => payment.UpdatedBy).IsRequired(false);
            entityTypeBuilder.Property(payment => payment.DeletedAt).IsRequired(false);
            entityTypeBuilder.Property(payment => payment.DeletedBy).IsRequired(false);
        }
    }
}
