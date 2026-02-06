using dttbidsmxbb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dttbidsmxbb.Data.Configurations
{
    public class MilitaryBaseConfiguration : IEntityTypeConfiguration<MilitaryBase>
    {
        public void Configure(EntityTypeBuilder<MilitaryBase> builder)
        {
            builder.Property(x => x.Name)
                   .IsRequired();
        }
    }
}
