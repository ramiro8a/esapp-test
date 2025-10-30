namespace PaymentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExternalOperationId).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.ExternalOperationId).IsUnique();
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
