using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Constants;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Mappings.Transactions
{
    internal sealed class TransactionRecordMapping : IEntityTypeConfiguration<TransactionRecord>
    {
        public void Configure(EntityTypeBuilder<TransactionRecord> builder)
        {
            builder.ToTable("tblTransaction");
            ConfigureProperties(builder);
            ConfigureKeys(builder);
        }

        private static void ConfigureKeys(EntityTypeBuilder<TransactionRecord> builder)
        {
            builder.HasKey(record => record.Id).HasName("PK_Transaction");
            builder.HasIndex(record => record.ExternalId).IsUnique().HasDatabaseName("UQ_Transaction_ExternalId");
            builder.HasIndex(record => new { record.CustomerId, record.OccurredAt, record.Id })
                .IsDescending(false, true, true)
                .HasDatabaseName("IX_Transaction_CustomerIdOccurredAtId");
        }

        private static void ConfigureProperties(EntityTypeBuilder<TransactionRecord> builder)
        {
            builder.Property(record => record.Id).HasColumnType(ColumnDataType.BigInt).ValueGeneratedOnAdd();
            builder.Property(record => record.ExternalId).HasColumnType(ColumnDataType.Uuid).IsRequired();
            builder.Property(record => record.CustomerId).HasColumnType(ColumnDataType.BigInt).IsRequired();
            builder.Property(record => record.OccurredAt).HasColumnType(ColumnDataType.TimestampWithTimeZone).IsRequired();
            builder.Property(record => record.Merchant).HasColumnType(ColumnDataType.Varchar100).IsRequired();
            builder.Property(record => record.Description).HasColumnType(ColumnDataType.Varchar200).IsRequired();
            builder.Property(record => record.AmountMinor).HasColumnType(ColumnDataType.BigInt).IsRequired();
            builder.Property(record => record.CurrencyCode).HasColumnType(ColumnDataType.Char3).IsRequired();
            builder.Property(record => record.State).HasColumnType(ColumnDataType.Varchar100).IsRequired();
        }
    }
}
