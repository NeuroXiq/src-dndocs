using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DNDocs.Domain.Entity.App;

namespace DNDocs.Infrastructure.Mapping.App
{
    internal class DJobRemoteServiceMap : IEntityTypeConfiguration<DJobRemoteService>
    {
        public void Configure(EntityTypeBuilder<DJobRemoteService> builder)
        {
            builder.ToTable("djob_remote_service");
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(m => m.InstanceName).HasColumnName("instance_name");
            builder.Property(m => m.ServerIpAddress).HasColumnName("server_ip_address");
            builder.Property(m => m.ServerPort).HasColumnName("server_port");
            builder.Property(m => m.Alive).HasColumnName("alive");
            builder.Property(m => m.CreatedOn).HasColumnName("created_on");
            builder.Property(m => m.UpdatedOn).HasColumnName("updated_on");

        }
    }
}
