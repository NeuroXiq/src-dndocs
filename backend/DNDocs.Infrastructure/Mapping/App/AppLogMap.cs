using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DNDocs.Domain.Entity.App;

namespace DNDocs.Infrastructure.Mapping.App
{
    internal class AppLogMap : IEntityTypeConfiguration<AppLog>
    {
        public void Configure(EntityTypeBuilder<AppLog> builder)
        {
            builder.ToTable("app_log");
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(m => m.EventId).HasColumnName("eventid");
            builder.Property(m => m.CategoryName).HasColumnName("category_name").IsRequired(false);
            builder.Property(m => m.EventName).HasColumnName("event_name").IsRequired(false);
            builder.Property(m => m.Message).HasColumnName("message").IsRequired(false);
            builder.Property(m => m.Date).HasColumnName("date").IsRequired(false);
            builder.Property(m => m.LogLevelId).HasColumnName("loglevelid");
            builder.Property(m => m.TraceId).HasColumnName("trace_id").IsRequired(false);
        }
    }
}
