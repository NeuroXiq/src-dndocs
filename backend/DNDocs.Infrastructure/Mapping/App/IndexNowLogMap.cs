using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DNDocs.Domain.Entity.App;

namespace DNDocs.Infrastructure.Mapping.App
{
    internal class IndexNowLogMap : IEntityTypeConfiguration<IndexNowLog>
    {
        public void Configure(EntityTypeBuilder<IndexNowLog> b)
        {
            b.ToTable("indexnow_log");
            b.HasKey(t => t.Id);

            b.Property(t => t.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            b.Property(t => t.LastException)
                .HasColumnName("last_exception")
                .IsRequired(false);

            b.Property(t => t.LastSubmitDate)
                .HasColumnName("last_submit_date")
                .IsRequired(true);

            b.Property(t => t.SiteItemIdEnd)
                .HasColumnName("site_item_id_end");
            
            b.Property(t => t.SiteItemIdStart).HasColumnName("site_item_id_start"); 

            b.Property(t => t.SubmitAttemptCount).HasColumnName("submit_attempt_count"); 

            b.Property(t => t.Success).HasColumnName("success"); 

        }
    }
}
