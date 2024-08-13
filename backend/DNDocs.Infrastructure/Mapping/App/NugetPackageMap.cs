using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DNDocs.Domain.Entity.App;

namespace DNDocs.Infrastructure.Mapping.App
{
    internal class NugetPackageMap : IEntityTypeConfiguration<NugetPackage>
    {
        public void Configure(EntityTypeBuilder<NugetPackage> builder)
        {
            builder.ToTable("nuget_package");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title)
                .HasColumnName("title")
                .IsRequired(false);

            builder.Property(t => t.IdentityVersion)
                .HasColumnName("identity_version")
                .IsRequired(false);

            builder.Property(t => t.IdentityId)
                .HasColumnName("identity_id")
                .IsRequired(false);

            builder.Property(t => t.PublishedDate)
                .HasColumnName("published_date")
                .IsRequired(false);

            builder.Property(t => t.ProjectUrl)
                .HasColumnName("project_url")
                .IsRequired(false);

            builder.Property(t => t.PackageDetailsUrl)
                .HasColumnName("package_details_url")
                .IsRequired(false);

            builder.Property(t => t.IsListed)
                .HasColumnName("is_listed");

            builder.Property(t => t.ProjectId)
                .HasColumnName("project_id")
                .IsRequired(false);

            builder.Property(t => t.ProjectVersioningId)
                .HasColumnName("project_versioning_id")
                .IsRequired(false);

            builder.HasOne(t => t.Project)
                .WithMany(t => t.ProjectNugetPackages)
                .HasForeignKey(t => t.ProjectId)
                .IsRequired(false);

            builder.HasOne(t => t.ProjectVersioning)
                .WithMany(t => t.NugetPackages)
                .HasForeignKey(t => t.ProjectVersioningId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
