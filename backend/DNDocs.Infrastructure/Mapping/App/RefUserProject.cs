using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DNDocs.Domain.Entity.App;

namespace DNDocs.Infrastructure.Mapping.App
{
    public class RefUserProjectMap : IEntityTypeConfiguration<RefUserProject>
    {
        public void Configure(EntityTypeBuilder<RefUserProject> builder)
        {
            builder.ToTable("ref_user_project");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(t => t.UserId)
                .HasColumnName("userid");

            builder.Property(t => t.ProjectId)
                .HasColumnName("project_id");
        }
    }
}
