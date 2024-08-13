
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DNDocs.Domain.Entity.App;

namespace DNDocs.Infrastructure.Mapping.App
{
    internal class BgJobMap : IEntityTypeConfiguration<BgJob>
    {
        public void Configure(EntityTypeBuilder<BgJob> builder)
        {
            builder.ToTable("bgjob");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasColumnName("id");

            builder.Property(t => t.QueuedDateTime)
                .HasColumnName("queued_datetime")
                .IsRequired(true);

            builder.Property(t => t.StartedDateTime)
                .HasColumnName("started_datetime")
                .IsRequired(false);

            builder.Property(t => t.CompletedDateTime)
                .HasColumnName("completed_datetime")
                .IsRequired(false);

            builder.Property(t => t.DoWorkCommandType)
                .HasColumnName("dowork_command_type")
                .IsRequired(false);

            builder.Property(t => t.DoWorkCommandData)
                .HasColumnName("dowork_command_data")
                .IsRequired(false);

            builder.Property(t => t.Status)
                .HasColumnName("status");

            builder.Property(t => t.ExecuteAsUserId)
                .HasColumnName("execute_as_user_id");

            builder.Property(t => t.CommandHandlerSuccess)
                .HasColumnName("command_handler_success")
                .IsRequired(false);

            builder.Property(t => t.CommandHandlerResult)
                .HasColumnName("command_handler_result")
                .IsRequired(false);

            builder.Property(t => t.Exception)
                .HasColumnName("exception")
                .IsRequired(false);

            builder.Property(t => t.ExeThreadId)
                .HasColumnName("exe_thread_id")
                .IsRequired(false);

            builder.Property(t => t.BuildsProjectId).HasColumnName("builds_project_id").IsRequired(false);
        }
    }
}