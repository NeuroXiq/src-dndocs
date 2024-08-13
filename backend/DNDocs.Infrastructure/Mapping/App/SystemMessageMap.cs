
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DNDocs.Domain.Entity.App;

namespace DNDocs.Infrastructure.Mapping.App
{
    internal class SystemMessageMap : IEntityTypeConfiguration<SystemMessage>
    {
        public void Configure(EntityTypeBuilder<SystemMessage> builder)
        {
            builder.ToTable("system_message");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Type)
                .HasColumnName("type");

            builder.Property(t => t.Level)
                .HasColumnName("level");

            builder.Property(t => t.Title)
                .IsRequired(false);

            builder.Property(t => t.Message)
                .IsRequired(false);

            builder.Property(t => t.ProjectId)
                .HasColumnName("project_id")
                .IsRequired(false);

            builder.Property(t => t.UserId)
                .HasColumnName("user_id")
                .IsRequired(false);

            builder.Property(t => t.DateTime)
                .HasColumnName("datetime");

            builder.Property(t => t.TraceBgJobId)
                .HasColumnName("trace_bgjob_id")
                .IsRequired(false);

            builder.Property(t => t.ProjectVersioningId)
                .HasColumnName("project_versioning_id")
                .IsRequired(false);

            builder.HasOne(t => t.ProjectVersioning)
                .WithMany(t => t.SystemMessages)
                .HasForeignKey(t => t.ProjectVersioningId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
