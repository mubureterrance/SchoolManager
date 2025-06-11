using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations
{
    public class FeePaymentConfiguration : IEntityTypeConfiguration<FeePayment>
    {
        public void Configure(EntityTypeBuilder<FeePayment> builder)
        {
            builder.HasKey(p => p.PaymentId);
            builder.Property(p => p.PaymentId).HasDefaultValueSql("NEWID()");
            builder.Property(p => p.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            builder.Property(p => p.Method)
                   .HasConversion<string>();

            builder.Property(p => p.Status)
                   .HasConversion<string>();

            builder.HasOne(p => p.Student)
                   .WithMany(s => s.FeePayments)
                   .HasForeignKey(p => p.StudentId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.FeeStructure)
                   .WithMany()
                   .HasForeignKey(p => p.FeeStructureId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
