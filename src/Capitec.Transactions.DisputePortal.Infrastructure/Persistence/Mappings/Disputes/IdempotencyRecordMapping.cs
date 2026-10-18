using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Constants;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Mappings.Disputes
{
    internal sealed class IdempotencyRecordMapping : IEntityTypeConfiguration<IdempotencyRecord>
    {
        public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
        {
            builder.ToTable("tblIdempotencyRecord");
            ConfigureProperties(builder);
            ConfigureKeys(builder);
        }

        private static void ConfigureKeys(EntityTypeBuilder<IdempotencyRecord> builder)
        {
            builder.HasKey(ShadowProperties.Id).HasName("PK_IdempotencyRecord");
            builder.HasIndex(record => new { record.CustomerId, record.Operation, record.IdempotencyKey })
                .IsUnique()
                .HasDatabaseName("UQ_IdempotencyRecord_CustomerIdOperationIdempotencyKey");
        }

        private static void ConfigureProperties(EntityTypeBuilder<IdempotencyRecord> builder)
        {
            builder.Property<long>(ShadowProperties.Id).HasColumnType(ColumnDataType.BigInt).ValueGeneratedOnAdd();
            builder.Property(record => record.CustomerId).HasColumnType(ColumnDataType.BigInt).IsRequired();
            builder.Property(record => record.Operation).HasColumnType(ColumnDataType.Varchar100).IsRequired();
            builder.Property(record => record.IdempotencyKey).HasColumnType(ColumnDataType.Varchar200).IsRequired();
            builder.Property(record => record.RequestHash).HasColumnType(ColumnDataType.Varchar100).IsRequired();
            builder.Property(record => record.DisputeExternalId).HasColumnType(ColumnDataType.Uuid).IsRequired();
            builder.Property(record => record.CreatedAt).HasColumnType(ColumnDataType.TimestampWithTimeZone).IsRequired();
        }
    }
}
