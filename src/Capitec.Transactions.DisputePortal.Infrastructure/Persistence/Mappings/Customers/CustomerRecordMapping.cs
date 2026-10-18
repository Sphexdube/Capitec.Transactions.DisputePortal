using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Constants;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Mappings.Customers
{
    internal sealed class CustomerRecordMapping : IEntityTypeConfiguration<CustomerRecord>
    {
        public void Configure(EntityTypeBuilder<CustomerRecord> builder)
        {
            builder.ToTable("tblCustomer");
            ConfigureProperties(builder);
            ConfigureKeys(builder);
        }

        private static void ConfigureKeys(EntityTypeBuilder<CustomerRecord> builder)
        {
            builder.HasKey(record => record.Id).HasName("PK_Customer");
            builder.HasIndex(record => record.ExternalId).IsUnique().HasDatabaseName("UQ_Customer_ExternalId");
        }

        private static void ConfigureProperties(EntityTypeBuilder<CustomerRecord> builder)
        {
            builder.Property(record => record.Id).HasColumnType(ColumnDataType.BigInt).ValueGeneratedOnAdd();
            builder.Property(record => record.ExternalId).HasColumnType(ColumnDataType.Uuid).IsRequired();
            builder.Property(record => record.DisplayName).HasColumnType(ColumnDataType.Varchar100).IsRequired();
        }
    }
}
