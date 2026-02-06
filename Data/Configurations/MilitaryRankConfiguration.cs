using dttbidsmxbb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dttbidsmxbb.Data.Configurations
{
    public class MilitaryRankConfiguration : IEntityTypeConfiguration<MilitaryRank>
    {
        public void Configure(EntityTypeBuilder<MilitaryRank> builder)
        {
            builder.Property(x => x.Name)
                   .IsRequired();
        }
    }
}
