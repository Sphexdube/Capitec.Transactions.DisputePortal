using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Mappings.Disputes
{
    internal sealed class DisputeMapping : IEntityTypeConfiguration<Dispute>
    {
        public void Configure(EntityTypeBuilder<Dispute> builder)
        {
            builder.ToTable("tblDispute");
            ConfigureProperties(builder);
            ConfigureKeys(builder);
        }

        private static void ConfigureKeys(EntityTypeBuilder<Dispute> builder)
        {
            builder.HasKey(dispute => dispute.Id).HasName("PK_Dispute");
            builder.HasIndex(dispute => dispute.ExternalId).IsUnique().HasDatabaseName("UQ_Dispute_ExternalId");
            builder.HasIndex(dispute => new { dispute.CustomerId, dispute.CreatedAt, dispute.Id })
                .IsDescending(false, true, true)
                .HasDatabaseName("IX_Dispute_CustomerIdCreatedAtId");
            builder.HasIndex(dispute => new { dispute.CustomerId, dispute.TransactionId })
                .IsUnique()
                .HasFilter("\"Status\" IN ('Submitted', 'UnderReview')")
                .HasDatabaseName("UQ_Dispute_CustomerIdTransactionId_Active");
        }

        private static void ConfigureProperties(EntityTypeBuilder<Dispute> builder)
        {
            builder.Property(dispute => dispute.Id).HasColumnType(ColumnDataType.BigInt).ValueGeneratedOnAdd();
            builder.Property(dispute => dispute.ExternalId).HasColumnType(ColumnDataType.Uuid).IsRequired();
            builder.Property(dispute => dispute.CustomerId).HasColumnType(ColumnDataType.BigInt).IsRequired();
            builder.Property(dispute => dispute.TransactionId).HasColumnType(ColumnDataType.BigInt).IsRequired();
            builder.Property(dispute => dispute.Reason).HasColumnType(ColumnDataType.Varchar100).IsRequired();
            builder.Property(dispute => dispute.Explanation).HasColumnType(ColumnDataType.Varchar500);
            builder.Property(dispute => dispute.Status).HasConversion<string>().HasColumnType(ColumnDataType.Varchar100).IsRequired();
            builder.Property(dispute => dispute.CreatedAt).HasColumnType(ColumnDataType.TimestampWithTimeZone).IsRequired();
            builder.Property(dispute => dispute.UpdatedAt).HasColumnType(ColumnDataType.TimestampWithTimeZone).IsRequired();
        }
    }
}
