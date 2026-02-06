using dttbidsmxbb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dttbidsmxbb.Data.Configurations
{
    public class AuthLogConfiguration : IEntityTypeConfiguration<AuthLog>
    {
        public void Configure(EntityTypeBuilder<AuthLog> builder)
        {
            builder.Property(x => x.Username).IsRequired().HasMaxLength(200);
            builder.Property(x => x.IpAddress).IsRequired().HasMaxLength(50);
            builder.Property(x => x.FailureReason).HasMaxLength(500);
            builder.Property(x => x.Timestamp).IsRequired();
            builder.HasIndex(x => x.Timestamp);
        }
    }
}