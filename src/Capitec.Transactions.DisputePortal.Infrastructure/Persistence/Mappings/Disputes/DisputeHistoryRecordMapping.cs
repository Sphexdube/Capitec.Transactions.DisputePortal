using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Constants;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Mappings.Disputes
{
    internal sealed class DisputeHistoryRecordMapping : IEntityTypeConfiguration<DisputeHistoryRecord>
    {
        public void Configure(EntityTypeBuilder<DisputeHistoryRecord> builder)
        {
            builder.ToTable("tblDisputeHistory");
            ConfigureProperties(builder);
            ConfigureKeys(builder);
        }

        private static void ConfigureKeys(EntityTypeBuilder<DisputeHistoryRecord> builder)
        {
            builder.HasKey(ShadowProperties.Id).HasName("PK_DisputeHistory");
            builder.HasIndex(nameof(DisputeHistoryRecord.DisputeId), nameof(DisputeHistoryRecord.OccurredAt), ShadowProperties.Id)
                .HasDatabaseName("IX_DisputeHistory_DisputeIdOccurredAtId");
        }

        private static void ConfigureProperties(EntityTypeBuilder<DisputeHistoryRecord> builder)
        {
            builder.Property<long>(ShadowProperties.Id).HasColumnType(ColumnDataType.BigInt).ValueGeneratedOnAdd();
            builder.Property(record => record.DisputeId).HasColumnType(ColumnDataType.BigInt).IsRequired();
            builder.Property(record => record.EventType).HasColumnType(ColumnDataType.Varchar100).IsRequired();
            builder.Property(record => record.PreviousStatus).HasColumnType(ColumnDataType.Varchar100);
            builder.Property(record => record.CurrentStatus).HasColumnType(ColumnDataType.Varchar100).IsRequired();
            builder.Property(record => record.OccurredAt).HasColumnType(ColumnDataType.TimestampWithTimeZone).IsRequired();
            builder.Property(record => record.Actor).HasColumnType(ColumnDataType.Varchar100).IsRequired();
        }
    }
}
