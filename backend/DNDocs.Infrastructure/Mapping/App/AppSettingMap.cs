using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DNDocs.Domain.Entity.App;

namespace DNDocs.Infrastructure.Mapping.App
{
    internal class AppSettingMap : IEntityTypeConfiguration<SysVar>
    {
        public void Configure(EntityTypeBuilder<SysVar> b)
        {
            b.ToTable("app_setting");
            b.HasKey(t => t.Id);

            b.Property(t => t.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            b.Property(t => t.Key)
                .HasColumnName("key");

            b.Property(t => t.Value)
                .HasColumnName("value");
        }
    }
}
