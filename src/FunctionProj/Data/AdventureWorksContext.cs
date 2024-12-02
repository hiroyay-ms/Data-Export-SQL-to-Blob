using Microsoft.EntityFrameworkCore;
using FunctionProj.Models;

namespace FunctionProj.Data;

public class AdventureWorksContext : DbContext
{
    public AdventureWorksContext(DbContextOptions<AdventureWorksContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Product { get; set; } = null!;
    public DbSet<ProductCategory> ProductCategory => Set<ProductCategory>();
    public DbSet<SalesOrderHeader> SalesOrderHeader => Set<SalesOrderHeader>();
    public DbSet<SalesOrderDetail> SalesOrderDetail => Set<SalesOrderDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("SalesLT");

        modelBuilder.Entity<Product>().ToTable("Product");
        modelBuilder.Entity<ProductCategory>().ToTable("ProductCategory");
        modelBuilder.Entity<SalesOrderHeader>().ToTable("SalesOrderHeader");
        modelBuilder.Entity<SalesOrderDetail>().ToTable("SalesOrderDetail");
    }
}