using Microsoft.EntityFrameworkCore;
using WebApiProj.Models;

namespace WebApiProj.Data;
public class AdventureWorksContext : DbContext
{
    public AdventureWorksContext(DbContextOptions<AdventureWorksContext> options) : base(options)
    {
    }

    public DbSet<ProductCategory> ProductCategory => Set<ProductCategory>();
    public DbSet<Product> Product => Set<Product>();
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