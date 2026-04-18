using EComAPI.Domain.Transaction.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Transaction.Persistence.Configurations
{
    public class OrderStatusLogConfiguration : IEntityTypeConfiguration<OrderStatusLog>
    {
        public void Configure(EntityTypeBuilder<OrderStatusLog> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("OrderStatusLogs");

            entityTypeBuilder.HasKey(log => log.Id);

            entityTypeBuilder.Property(log => log.OrderId).IsRequired();

            entityTypeBuilder.Property(log => log.Status)
                .IsRequired()
                .HasMaxLength(50);

            entityTypeBuilder.Property(log => log.Note)
                .HasMaxLength(500)
                .IsRequired(false);

            entityTypeBuilder.Property(log => log.ChangedAt).IsRequired();
            entityTypeBuilder.Property(log => log.CreatedAt).IsRequired();
            entityTypeBuilder.Property(log => log.CreatedBy).IsRequired(false);

            entityTypeBuilder.HasIndex(log => log.OrderId);
        }
    }
}
