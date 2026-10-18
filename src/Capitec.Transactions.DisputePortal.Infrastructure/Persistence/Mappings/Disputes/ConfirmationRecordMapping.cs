using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Constants;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Mappings.Disputes
{
    internal sealed class ConfirmationRecordMapping : IEntityTypeConfiguration<ConfirmationRecord>
    {
        public void Configure(EntityTypeBuilder<ConfirmationRecord> builder)
        {
            builder.ToTable("tblConfirmation");
            ConfigureProperties(builder);
            ConfigureKeys(builder);
        }

        private static void ConfigureKeys(EntityTypeBuilder<ConfirmationRecord> builder)
        {
            builder.HasKey(ShadowProperties.Id).HasName("PK_Confirmation");
            builder.HasIndex(record => record.OutboxEventId).IsUnique().HasDatabaseName("UQ_Confirmation_OutboxEventId");
            builder.HasIndex(record => record.DisputeId).HasDatabaseName("IX_Confirmation_DisputeId");
        }

        private static void ConfigureProperties(EntityTypeBuilder<ConfirmationRecord> builder)
        {
            builder.Property<long>(ShadowProperties.Id).HasColumnType(ColumnDataType.BigInt).ValueGeneratedOnAdd();
            builder.Property(record => record.DisputeId).HasColumnType(ColumnDataType.BigInt).IsRequired();
            builder.Property(record => record.OutboxEventId).HasColumnType(ColumnDataType.Uuid).IsRequired();
            builder.Property(record => record.Message).HasColumnType(ColumnDataType.Varchar200).IsRequired();
            builder.Property(record => record.CreatedAt).HasColumnType(ColumnDataType.TimestampWithTimeZone).IsRequired();
        }
    }
}
