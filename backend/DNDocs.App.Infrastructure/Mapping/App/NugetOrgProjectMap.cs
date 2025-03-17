using DNDocs.App.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Infrastructure.Mapping.App
{
    internal class NugetOrgProjectMap : IEntityTypeConfiguration<NugetOrgProject>
    {
        public void Configure(EntityTypeBuilder<NugetOrgProject> b)
        {
            b.ToTable("nugetorg_project");
            b.HasKey(t => t.Id);

            b.Property(t => t.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            b.Property(t => t.PackageName)
                .HasColumnName("package_name");

            b.Property(t => t.PackageVersion)
                .HasColumnName("package_version");

            b.Property(t => t.IsOnline)
                .HasColumnName("is_online");

            b.Property(t => t.State)
                .HasColumnName("state");

            b.Property(t => t.BuildStartOn)
                .HasColumnName("build_starton");

            b.Property(t => t.CreatedOn)
                .HasColumnName("created_on");

            b.Property(t => t.LastModifiedOn)
                .HasColumnName("last_modified_on");
        }
    }
}
