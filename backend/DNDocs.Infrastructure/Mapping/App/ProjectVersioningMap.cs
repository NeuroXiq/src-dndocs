using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DNDocs.Domain.Entity.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Infrastructure.Mapping.App
{
    internal class ProjectVersioningMap : IEntityTypeConfiguration<ProjectVersioning>
    {
        public void Configure(EntityTypeBuilder<ProjectVersioning> builder)
        {
            builder.ToTable("project_versioning");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("id");
            builder.Property(t => t.ProjectName).HasColumnName("project_name").IsRequired(false);
            builder.Property(t => t.ProjectWebsiteUrl).HasColumnName("project_website_url").IsRequired(false);
            builder.Property(t => t.UrlPrefix).HasColumnName("url_prefix").IsRequired(true);
            builder.Property(t => t.GitDocsRepoUrl).HasColumnName("git_docs_repo_url").IsRequired(false);
            builder.Property(t => t.GitDocsBranchName).HasColumnName("git_docs_branch_name").IsRequired(false);
            builder.Property(t => t.GitDocsRelativePath).HasColumnName("git_docs_relative_path").IsRequired(false);
            builder.Property(t => t.GitHomepageRelativePath).HasColumnName("git_homepage_relative_path").IsRequired(false);
            builder.Property(t => t.UserId).HasColumnName("user_id").IsRequired(true);
            builder.Property(t => t.Autoupgrage).HasColumnName("autoupgrade").IsRequired(true);
            builder.Property(t => t.LastAutoupgradeAt).HasColumnName("last_autoupgrade_at").IsRequired(false);
            builder.Property(t => t.LastAutoupgradeError).HasColumnName("last_autoupgrade_error").IsRequired(false);
            builder.Property(t => t.Autoupgrage).HasColumnName("autoupgrade").IsRequired(true);

            builder.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .IsRequired(true);
        }
    }
}
