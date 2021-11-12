using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Capstone.Common.Scheduler.Models
{
    public partial  class ProdaDBContext : DbContext
    {
        public ProdaDBContext(DbContextOptions<ProdaDBContext> options)
        : base(options)
        {
        }
        public ProdaDBContext()
        {

        }
      
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var sqlConnection = config["ConnectionStrings:dev-common-dbcs"];
            

            optionsBuilder.UseSqlServer(sqlConnection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

          
            modelBuilder.Entity<ProdaTokenProperty>(entity =>
            {
                entity.HasKey(e => new { e.TenantId, e.LocationId })
                    .HasName("PK__tmp_ms_x__40E4ADA87F05FA4F");

                entity.Property(e => e.LocationId).HasMaxLength(50);

                entity.Property(e => e.Created)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(sysdatetime())");

                entity.Property(e => e.DeviceName).HasMaxLength(50);

                entity.Property(e => e.Modified).HasColumnType("datetime");

                entity.Property(e => e.TokenExpiry).HasColumnType("datetime");
            });
          
           
        }



        public virtual DbSet<ProdaTokenProperty> ProdaTokenProperties { get; set; }
       
    }
}