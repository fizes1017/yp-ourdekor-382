using Microsoft.EntityFrameworkCore;
using ourdekor.Models;

namespace ourdekor.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<MaterialType> MaterialTypes { get; set; }
        public DbSet<Materials> Materials { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<ProductMaterials> ProductMaterials { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MaterialType>(entity =>
            {
                entity.HasKey(e => e.id);
            });

            modelBuilder.Entity<Materials>(entity =>
            {
                entity.HasKey(e => e.id);

                entity.HasOne(e => e.MaterialType)
                    .WithMany()
                    .HasForeignKey(e => e.MaterialTypeId);
            });

            modelBuilder.Entity<ProductType>(entity =>
            {
                entity.HasKey(e => e.id);
            });

            modelBuilder.Entity<Products>(entity =>
            {
                entity.HasKey(e => e.id);

                entity.HasOne(e => e.ProductType)
                    .WithMany()
                    .HasForeignKey(e => e.ProductTypeId);
            });

            modelBuilder.Entity<ProductMaterials>(entity =>
            {
                entity.HasKey(e => e.id);

                entity.HasOne(e => e.Materials)
                    .WithMany()
                    .HasForeignKey(e => e.MaterialId);

                entity.HasOne(e => e.Products)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId);
            });
        }
    }
}
