using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DNDocs.Domain.Entity.App;

namespace DNDocs.Infrastructure.Mapping.App
{
    internal class ProjectMap : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.ProjectName)
                .HasColumnName("project_name");

            builder.Property(t => t.State)
                .HasColumnName("state");

            builder.Property(t => t.StateDetails)
                .HasColumnName("state_details");

            builder.Property(t => t.Description)
                .HasColumnName("description")
                .IsRequired(false);

            builder.Property(t => t.UrlPrefix)
                .HasColumnName("url_prefix")
                .IsRequired(false);

            builder.Property(t => t.GithubUrl)
                .HasColumnName("githuburl")
                .IsRequired(false);

            builder.Property(t => t.Comment)
                .HasColumnName("comment")
                .IsRequired(false);

            builder.Property(t => t.LastBuildStartOn)
                .HasColumnName("last_build_start_on")
                .IsRequired(false);

            builder.Property(t => t.LastBuildCompletedOn)
                .HasColumnName("last_build_completed_on")
                .IsRequired(false);

            builder.Property(t => t.BgHealthCheckHttpGetDateTime)
                .HasColumnName("bghealthcheck_httpget_datetime")
                .IsRequired(false);

            builder.Property(t => t.BgHealthCheckHttpGetStatus)
                .HasColumnName("bghealthcheck_httpget_status")
                .IsRequired(false);

            builder.Property(t => t.LastBuildErrorLog)
                .HasColumnName("last_build_error_log")
                .IsRequired(false);

            builder.Property(t => t.NupkgAutorebuildLastDateTime)
                .HasColumnName("nupkg_autorebuild_last_datetime")
                .IsRequired(false);

            builder.Property(t => t.CreatedOn)
                .HasColumnName("created_on");

            builder.Property(t => t.LastModifiedOn)
                .HasColumnName("last_modified_on");

            builder.Property(t => t.GitMdRepoUrl)
                .HasColumnName("git_md_repo_url")
                .IsRequired(false);

            builder.Property(t => t.GitMdBranchName)
                .HasColumnName("git_md_branch_name")
                .IsRequired(false);

            builder.Property(t => t.GitMdRelativePathDocs)
                .HasColumnName("git_md_relative_path_docs")
                .IsRequired(false);

            builder.Property(t => t.GitMdRelativePathReadme)
                .HasColumnName("git_md_relative_path_readme")
                .IsRequired(false);

            builder.Property(t => t.PSAutorebuild)
                .HasColumnName("ps_autorebuild")
                .IsRequired(true);

            builder.Property(t => t.GitDocsCommitHash)
                .HasColumnName("git_docs_commit_hash")
                .IsRequired(false);

            builder.Property(t => t.DocfxTemplate)
                .HasColumnName("docfx_template")
                .IsRequired(false);

            builder.Property(t => t.PVGitTag)
                .HasColumnName("pv_git_tag")
                .IsRequired(false);

            builder.Property(t => t.PVProjectVersioningId)
                .HasColumnName("pv_project_versioning_id")
                .IsRequired(false);

            builder.Property(t => t.NugetOrgPackageName)
                .HasColumnName("nugetorg_package_name")
                .IsRequired(false);

            builder.Property(t => t.NugetOrgPackageVersion)
                .HasColumnName("nugetorg_package_version")
                .IsRequired(false);

            builder.Property(t => t.ProjectType)
                .HasColumnName("project_type");

            builder.HasMany(t => t.RefUserProject)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.PVProjectVersioning)
                .WithMany(t => t.Projects)
                .HasForeignKey(t => t.PVProjectVersioningId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(t => t.SystemMessages)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        }
    }
}
