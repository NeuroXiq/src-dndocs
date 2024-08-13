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
    internal class GitRepoStoreMap : IEntityTypeConfiguration<GitRepoStore>
    {
        public void Configure(EntityTypeBuilder<GitRepoStore> builder)
        {
            builder.ToTable("git_repo_store");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasColumnName("id");
            builder.Property(t => t.LastAccessOn).HasColumnName("last_access_on").IsRequired(true);
            builder.Property(t => t.LastModifiedOn).HasColumnName("last_modified_on").IsRequired(true);
            builder.Property(t => t.CreatedOn).HasColumnName("created_on").IsRequired(true);
            builder.Property(t => t.GitRepoUrl).HasColumnName("git_repo_url").IsRequired(true);
        }
    }
}
