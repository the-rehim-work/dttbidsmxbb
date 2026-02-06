using dttbidsmxbb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dttbidsmxbb.Data.Configurations
{
    public class ExecutorConfiguration : IEntityTypeConfiguration<Executor>
    {
        public void Configure(EntityTypeBuilder<Executor> builder)
        {
            builder.Property(e => e.FullInfo)
                   .IsRequired();
        }
    }
}
