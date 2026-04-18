using EComAPI.Domain.Transaction.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Transaction.Persistence.Configurations
{
    public class StockLogConfiguration : IEntityTypeConfiguration<StockLog>
    {
        public void Configure(EntityTypeBuilder<StockLog> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("StockLogs");

            entityTypeBuilder.HasKey(stockLog => stockLog.Id);

            entityTypeBuilder.Property(stockLog => stockLog.ProductVariantId).IsRequired();

            entityTypeBuilder.Property(stockLog => stockLog.Type)
                .IsRequired()
                .HasMaxLength(20);

            entityTypeBuilder.Property(stockLog => stockLog.QuantityChange).IsRequired();
            entityTypeBuilder.Property(stockLog => stockLog.StockBefore).IsRequired();
            entityTypeBuilder.Property(stockLog => stockLog.StockAfter).IsRequired();

            entityTypeBuilder.Property(stockLog => stockLog.ReferenceType)
                .HasMaxLength(50)
                .IsRequired(false);

            entityTypeBuilder.Property(stockLog => stockLog.ReferenceId).IsRequired(false);

            entityTypeBuilder.Property(stockLog => stockLog.Note)
                .HasMaxLength(500)
                .IsRequired(false);

            entityTypeBuilder.Property(stockLog => stockLog.CreatedAt).IsRequired();
            entityTypeBuilder.Property(stockLog => stockLog.CreatedBy).IsRequired(false);

            entityTypeBuilder.HasIndex(stockLog => stockLog.ProductVariantId);
            entityTypeBuilder.HasIndex(stockLog => stockLog.ReferenceId);
        }
    }
}
