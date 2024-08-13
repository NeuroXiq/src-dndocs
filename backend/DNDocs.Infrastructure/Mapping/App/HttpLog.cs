using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DNDocs.Domain.Entity.App;

namespace DNDocs.Infrastructure.Mapping.App
{
    internal class HttpLogMap : IEntityTypeConfiguration<HttpLog>
    {
        public void Configure(EntityTypeBuilder<HttpLog> builder)
        {
            builder.ToTable("http_log");
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(m => m.Method).HasColumnName("method").IsRequired(false);
            builder.Property(m => m.Path).HasColumnName("path").IsRequired(false);
            builder.Property(m => m.Headers).HasColumnName("headers").IsRequired(false);
            builder.Property(m => m.IP).HasColumnName("ip").IsRequired(false);
            builder.Property(m => m.DateTime).HasColumnName("datetime");
            builder.Property(m => m.Payload).HasColumnName("payload").IsRequired(false);
        }
    }
}
