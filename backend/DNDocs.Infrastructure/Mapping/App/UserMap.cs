using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DNDocs.Domain.Entity.App;

namespace DNDocs.Infrastructure.Mapping.App
{
    public class UserMap : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("user");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.CreatedOn)
                .HasColumnName("created_on")
                .IsRequired(true);

            builder.Property(t => t.LastModifiedOn)
                .HasColumnName("last_modified_on")
                .IsRequired(true);

            builder.Property(t => t.Login)
                .HasColumnName("login")
                .IsRequired(true);

            builder.Property(t => t.PrimaryEmail)
                .HasColumnName("primary_email")
                .IsRequired(true);

            builder.Property(t => t.GithubPrimaryEmail)
                .HasColumnName("github_primary_email")
                .IsRequired(false);

            builder.Property(t => t.GithubId)
                .HasColumnName("github_id")
                .IsRequired(false);

            builder.Property(t => t.GithubLogin)
                .HasColumnName("github_login")
                .IsRequired(false);

            builder.Property(t => t.GithubReposUrl)
                .HasColumnName("github_repos_url")
                .IsRequired(false);

            builder.Property(t => t.GithubUrl)
                .HasColumnName("github_url")
                .IsRequired(false);

            builder.Property(t => t.GithubHtmlUrl)
                .HasColumnName("github_html_url")
                .IsRequired(false);

            builder.Property(t => t.GithubAvatarUrl)
                .HasColumnName("github_avatar_url")
                .IsRequired(false);

            builder.Property(t => t.GithubType)
                .HasColumnName("github_type")
                .IsRequired(false);

            builder.HasMany(t => t.RefUserProject)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId);

            builder.HasMany(t => t.SystemMessages)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            builder.HasMany(t => t.CreatedBgJobs)
                .WithOne(t => t.CreatedByUser)
                .HasForeignKey(t => t.ExecuteAsUserId)
                .IsRequired(false);
        }
    }
}
