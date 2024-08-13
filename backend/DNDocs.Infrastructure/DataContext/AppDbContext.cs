using Microsoft.EntityFrameworkCore;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Entity.Shared;
using DNDocs.Infrastructure.Mapping.App;

namespace DNDocs.Infrastructure.DataContext
{
    public class AppDbContext : DbContext
    {
        public DbSet<AppLog> AppLog { get; set; }
        public DbSet<HttpLog> HttpLog { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<BgJob> BgJob { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var appEntitiesNamespace = typeof(HttpLogMap).Namespace;

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(AppDbContext).Assembly,
                f => f.Namespace == appEntitiesNamespace);
        }

        public override int SaveChanges()
        {
            var cuEntityEntry = ChangeTracker
                .Entries()
                .Where(t => (t.State == EntityState.Added || t.State == EntityState.Modified) &&
                    t.Entity is ICreateUpdateTimestamp)
                .ToList();

            foreach (var cu in cuEntityEntry)
            {
                var entity = ((ICreateUpdateTimestamp)cu.Entity);
                entity.LastModifiedOn = DateTime.UtcNow;
                
                if (cu.State == EntityState.Added) entity.CreatedOn = DateTime.UtcNow;
            }

            return base.SaveChanges();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            //options.UseSqlite(
        }
    }
}
