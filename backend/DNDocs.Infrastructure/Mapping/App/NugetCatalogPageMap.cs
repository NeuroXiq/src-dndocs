using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DNDocs.Domain.Entity.App;

namespace DNDocs.Infrastructure.Mapping.App
{
    internal class NugetCatalogPageMap : IEntityTypeConfiguration<NugetCatalogPage>
    {
        public void Configure(EntityTypeBuilder<NugetCatalogPage> b)
        {
            b.ToTable("nuget_catalog_page");
            b.HasKey(t => t.Id);

            b.Property(t => t.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            b.Property(t => t.NId).HasColumnName("nid");
            b.Property(t => t.CommitId).HasColumnName("commit_id");
            b.Property(t => t.CommitTimeStamp).HasColumnName("commit_timestamp");
            b.Property(t => t.Count).HasColumnName("count");
            b.Property(t => t.State).HasColumnName("state");
            b.Property(t => t.CreatedOn).HasColumnName("created_on");
            b.Property(t => t.LastModifiedOn).HasColumnName("last_modified_on");
        }
    }
}
