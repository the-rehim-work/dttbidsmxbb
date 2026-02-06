using dttbidsmxbb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dttbidsmxbb.Data.Configurations
{
    public class EventLogConfiguration : IEntityTypeConfiguration<EventLog>
    {
        public void Configure(EntityTypeBuilder<EventLog> builder)
        {
            builder.Property(x => x.Method).IsRequired().HasMaxLength(10);
            builder.Property(x => x.Path).IsRequired().HasMaxLength(500);
            builder.Property(x => x.IpAddress).IsRequired().HasMaxLength(50);
            builder.Property(x => x.UserFullName).HasMaxLength(200);
            builder.Property(x => x.Timestamp).IsRequired();
            builder.HasIndex(x => x.Timestamp);
        }
    }
}