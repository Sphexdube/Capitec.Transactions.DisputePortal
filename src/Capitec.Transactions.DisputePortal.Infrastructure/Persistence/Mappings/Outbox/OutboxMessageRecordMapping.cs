using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Constants;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Mappings.Outbox
{
    internal sealed class OutboxMessageRecordMapping : IEntityTypeConfiguration<OutboxMessageRecord>
    {
        public void Configure(EntityTypeBuilder<OutboxMessageRecord> builder)
        {
            builder.ToTable("tblOutboxMessage");
            ConfigureProperties(builder);
            ConfigureKeys(builder);
        }

        private static void ConfigureKeys(EntityTypeBuilder<OutboxMessageRecord> builder)
        {
            builder.HasKey(ShadowProperties.Id).HasName("PK_OutboxMessage");
            builder.HasIndex(record => record.EventId).IsUnique().HasDatabaseName("UQ_OutboxMessage_EventId");
            builder.HasIndex(nameof(OutboxMessageRecord.NextAttemptAt), ShadowProperties.Id).HasDatabaseName("IX_OutboxMessage_NextAttemptAtId");
        }

        private static void ConfigureProperties(EntityTypeBuilder<OutboxMessageRecord> builder)
        {
            builder.Property<long>(ShadowProperties.Id).HasColumnType(ColumnDataType.BigInt).ValueGeneratedOnAdd();
            builder.Property(record => record.EventId).HasColumnType(ColumnDataType.Uuid).IsRequired();
            builder.Property(record => record.EventType).HasColumnType(ColumnDataType.Varchar100).IsRequired();
            builder.Property(record => record.Payload).HasColumnType(ColumnDataType.Jsonb).IsRequired();
            builder.Property(record => record.CreatedAt).HasColumnType(ColumnDataType.TimestampWithTimeZone).IsRequired();
            builder.Property(record => record.NextAttemptAt).HasColumnType(ColumnDataType.TimestampWithTimeZone).IsRequired();
            builder.Property<int>(ShadowProperties.Attempts).HasColumnType(ColumnDataType.Integer).IsRequired();
        }
    }
}
