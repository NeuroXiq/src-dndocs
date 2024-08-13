using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DNDocs.Domain.Entity.App;

namespace DNDocs.Infrastructure.Mapping.App
{
    public class CacheMap : IEntityTypeConfiguration<Cache>
    {
        public void Configure(EntityTypeBuilder<Cache> builder)
        {
            builder.ToTable("cache");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("id");

            builder.Property(t => t.CreatedOn)
                .HasColumnName("created_on")
                .IsRequired(true);

            builder.Property(t => t.LastModifiedOn)
                .HasColumnName("last_modified_on")
                .IsRequired(true);

            builder.Property(t => t.Key)
                .HasColumnName("key")
                .IsRequired(true);

            builder.Property(t => t.Data)
                .HasColumnName("data")
                .IsRequired(false);

            builder.Property(t => t.Expiration)
                .HasColumnName("expiration")
                .IsRequired(true);
        }
    }
}
