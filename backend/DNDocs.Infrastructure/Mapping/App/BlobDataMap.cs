//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using DNDocs.Domain.Entity.App;

//namespace DNDocs.Infrastructure.Mapping.App
//{
//    internal class BlobDataMap : IEntityTypeConfiguration<BlobData>
//    {
//        public void Configure(EntityTypeBuilder<BlobData> builder)
//        {
//            builder.ToTable("blob_data");
//            builder.HasKey(t => t.Id);
//            builder.Property(t => t.CreatedOn).HasColumnName("createdon");
//            builder.Property(t => t.LastModified).HasColumnName("lastmodified");
//            builder.Property(t => t.OriginalName)
//                .HasColumnName("original_name")
//                .IsRequired(false);

//            builder.HasMany(t => t.RefBlobDataProject)
//                .WithOne(t => t.BlobData)
//                .HasForeignKey(t => t.BlobDataId);
//        }
//    }
//}
