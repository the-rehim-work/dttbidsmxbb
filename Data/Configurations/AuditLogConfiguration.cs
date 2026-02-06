using dttbidsmxbb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dttbidsmxbb.Data.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.Property(x => x.UserFullName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Action).IsRequired().HasMaxLength(50);
            builder.Property(x => x.EntityName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Timestamp).IsRequired();
            builder.HasIndex(x => x.Timestamp);
            builder.HasIndex(x => x.EntityId);
        }
    }
}