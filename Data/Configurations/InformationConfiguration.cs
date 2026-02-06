using dttbidsmxbb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dttbidsmxbb.Data.Configurations
{
    public class InformationConfiguration : IEntityTypeConfiguration<Information>
    {
        public void Configure(EntityTypeBuilder<Information> builder)
        {
            builder.Property(x => x.SentSerialNumber)
                   .IsRequired();

            builder.Property(x => x.SentDate)
                    .IsRequired();

            builder.Property(x => x.ReceivedSerialNumber)
                    .IsRequired();

            builder.Property(x => x.ReceivedDate)
                    .IsRequired();

            builder.Property(x => x.RegardingPosition)
                    .IsRequired();

            builder.Property(x => x.Position)
                    .IsRequired();

            builder.Property(x => x.Lastname)
                    .IsRequired(false);

            builder.Property(x => x.Firstname)
                    .IsRequired();

            builder.Property(x => x.Fathername)
                    .IsRequired(false);

            builder.Property(x => x.AssignmentDate)
                    .IsRequired();

            builder.Property(x => x.PrivacyLevel)
                    .IsRequired();

            builder.Property(x => x.SendAwaySerialNumber)
                    .IsRequired();

            builder.Property(x => x.SendAwayDate)
                    .IsRequired();

            builder.Property(x => x.FormalizationSerialNumber)
                    .IsRequired();

            builder.Property(x => x.FormalizationDate)
                    .IsRequired();

            builder.Property(x => x.RejectionInfo)
                    .IsRequired(false);

            builder.Property(x => x.SentBackInfo)
                    .IsRequired(false);

            builder.Property(x => x.Note)
                    .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                    .IsRequired();

            builder.Property(x => x.DeletedAt)
                    .IsRequired(false);

            builder.Property(x => x.ModifiedAt)
                    .IsRequired(false);

            builder.HasOne(x => x.SenderMilitaryBase)
                .WithMany()
                .HasForeignKey(x => x.SenderMilitaryBaseId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.MilitaryBase)
                .WithMany()
                .HasForeignKey(x => x.MilitaryBaseId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.Executor)
               .WithMany()
               .HasForeignKey(x => x.ExecutorId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
        }
    }
}
