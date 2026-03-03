using MetalStructuresAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace MetalStructuresAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<Calculation> Calculations { get; set; }
    public DbSet<CalculationItem> CalculationItems { get; set; }
    public DbSet<CommercialProposal> CommercialProposals { get; set; }
    public DbSet<CompanyInfo> CompanyInfo { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email)
                  .IsUnique();
            entity.HasIndex(e => e.Phone)
                  .IsUnique();
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasIndex(e => e.Article)
                  .IsUnique();
            
            entity.HasOne(m => m.Creator)
                  .WithMany()
                  .HasForeignKey(m => m.CreatedBy)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(m => m.Updater)
                  .WithMany()
                  .HasForeignKey(m => m.UpdatedBy)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Calculation>(entity =>
        {
            entity.HasMany(c => c.CalculationItems)
                  .WithOne(ci => ci.Calculation)
                  .HasForeignKey(ci => ci.CalculationId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(c => c.Manager)
                  .WithMany()
                  .HasForeignKey(c => c.ManagerId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(c => c.Updater)
                  .WithMany()
                  .HasForeignKey(c => c.UpdatedBy)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CalculationItem>(entity =>
        {
            entity.HasOne(ci => ci.Material)
                  .WithMany()
                  .HasForeignKey(ci => ci.MaterialId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(ci => ci.Creator)
                  .WithMany()
                  .HasForeignKey(ci => ci.CreatedBy)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(ci => ci.Updater)
                  .WithMany()
                  .HasForeignKey(ci => ci.UpdatedBy)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CommercialProposal>(entity =>
        {
            entity.HasOne(cp => cp.Calculation)
                  .WithMany()
                  .HasForeignKey(cp => cp.CalculationId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(cp => cp.Manager)
                  .WithMany()
                  .HasForeignKey(cp => cp.ManagerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasOne(a => a.User)
                  .WithMany()
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}


