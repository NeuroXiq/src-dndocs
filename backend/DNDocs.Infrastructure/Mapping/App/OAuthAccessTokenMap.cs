using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DNDocs.Domain.Entity.App;

namespace DNDocs.Infrastructure.Mapping.App
{
    public class OAuthAccessTokenMap : IEntityTypeConfiguration<OAuthAccessToken>
    {
        public void Configure(EntityTypeBuilder<OAuthAccessToken> builder)
        {
            builder.ToTable("oauth_access_token");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("id");

            builder.Property(t => t.UserId)
                .HasColumnName("user_id");

            builder.Property(t => t.AccessToken)
                .HasColumnName("access_token")
                .IsRequired(false);

            builder.Property(t => t.TokenType)
                .HasColumnName("token_type")
                .IsRequired(false);

            builder.Property(t => t.Scope)
                .HasColumnName("scope")
                .IsRequired(false);
        }
    }
}
