using Capstone.Common.Medicare.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Capstone.Common.Medicare
{
    public partial  class TenantDBContext : DbContext
    {
        public TenantDBContext(DbContextOptions<ApplicationDBContext> options)
        : base(options)
        {
        }
        public TenantDBContext()
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
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<TenantAzureProperty>(entity =>
            {
                entity.HasKey(e => new { e.TenantId, e.LocationId })
                    .HasName("PK__TenantAz__40E4ADA6BEFFC156");

                entity.Property(e => e.LocationId)
                    .HasMaxLength(50)
                    .HasColumnName("LocationID");

                entity.Property(e => e.AzureTenantId)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CertificateName).HasMaxLength(50);

                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.SecretKey)
                    .IsRequired()
                    .HasMaxLength(50);
            });
            modelBuilder.Entity<TenantMedicareProperty>(entity =>
            {
                entity.HasKey(e => new { e.TenantId, e.LocationId })
                    .HasName("PK__TenantMe__40E4ADA876A132CA");
                entity.Property(e => e.TenantId).ValueGeneratedNever();
                entity.Property(e => e.LocationId).HasMaxLength(20);

                entity.Property(e => e.Apikey)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("APIKey");

                entity.Property(e => e.Apisecret)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("APISecret");

                entity.Property(e => e.ApplicationName)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("ClientID");

                entity.Property(e => e.Created)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(sysdatetime())");

                entity.Property(e => e.DeviceActivationCode)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.DeviceName)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.ProdaOrgRa)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("ProdaOrgRA");
            });
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
            modelBuilder.Entity<ProdaRequestLog>(entity =>
            {
                entity.HasKey(e => e.MessageId);

                entity.Property(e => e.MessageId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Created)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(sysdatetime())");

                entity.Property(e => e.LocationId)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Modified).HasColumnType("datetime");

               entity.Property(e => e.Status).HasMaxLength(50);
            });
            modelBuilder.Entity<ProdaRequestLog>(entity =>
            {
                entity.HasKey(e => e.MessageId);

                entity.Property(e => e.MessageId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Created)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(sysdatetime())");

                entity.Property(e => e.LocationId)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Modified).HasColumnType("datetime");

                entity.Property(e => e.Response).HasMaxLength(50);

                entity.Property(e => e.Status).HasMaxLength(50);
            });

            modelBuilder.Entity<MedicareTokenProperty>(entity =>
            {
                entity.HasKey(e => new { e.TenantId, e.LocationId })
                    .HasName("PK__Medicare__40E4ADA8A0D0F5AD");

                entity.Property(e => e.LocationId).HasMaxLength(50);

                entity.Property(e => e.Created).HasDefaultValueSql("(sysdatetime())");
            });
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);


        public virtual DbSet<TenantAzureProperty> TenantAzureProperties { get; set; }
        public virtual DbSet<TenantMedicareProperty> TenantMedicareProperties { get; set; }
        public virtual DbSet<ProdaTokenProperty> ProdaTokenProperties { get; set; }
        public virtual DbSet<ProdaRequestLog> ProdaRequestLogs { get; set; }
        public virtual DbSet<MedicareTokenProperty> MedicareTokenProperties { get; set; }

    }
}