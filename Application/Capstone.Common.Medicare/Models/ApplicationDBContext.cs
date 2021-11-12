using Capstone.Common.Medicare.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;

using System.Configuration;

namespace Capstone.Common.Medicare
{
    public partial  class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
        : base(options)
        {
        }
        public ApplicationDBContext()
        {

        }
      
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("Default"));
        }
        public virtual DbSet<MedicareRequestLog> MedicareRequestLogs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

          
            modelBuilder.Entity<MedicareRequestLog>(entity =>
            {
                entity.HasKey(e => e.MessageId)
                    .HasName("PK__Medicare__C87C0C9C8F7707D7");

                entity.Property(e => e.MessageId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.ClaimId).HasMaxLength(50);

                entity.Property(e => e.CorrelationId)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Created)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(sysdatetime())");

                entity.Property(e => e.Modified).HasColumnType("datetime");

                entity.Property(e => e.Status).HasMaxLength(50);
            });
           


            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }
}