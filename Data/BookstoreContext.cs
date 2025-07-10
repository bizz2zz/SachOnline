using LuongVinhKhang.SachOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace LuongVinhKhang.SachOnline.Data
{
    public class BookstoreContext : DbContext
    {
        public BookstoreContext(DbContextOptions<BookstoreContext> options)
            : base(options) { }

        public DbSet<Product> Product { get; set; }
        public DbSet<NhaXuatBan> NhaXuatBan { get; set; }
        public DbSet<ChuDe> ChuDe { get; set; }
        public DbSet<Slider> Slider { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Cart> Cart { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.ChuDe)
                .WithMany(c => c.Product)
                .HasForeignKey(p => p.ChuDeId)
                .HasConstraintName("FK_Product_ChuDe");

            modelBuilder.Entity<Product>()
                .HasOne(p => p.NhaXuatBan)
                .WithMany(n => n.Product)
                .HasForeignKey(p => p.NhaXuatBanId)
                .HasConstraintName("FK_Product_NhaXuatBan");
            modelBuilder.Entity<Product>()
                .Property(p => p.SoLuongBan)
                .HasColumnName("soluongban")
                .HasDefaultValue(0);
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(e => e.CartId);
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.MaKH);
                entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId);
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.MaKH);
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => e.OrderDetailId);
                entity.HasOne(e => e.Order).WithMany(o => o.OrderDetails).HasForeignKey(e => e.OrderId);
                entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId);
            });
        }
    }
}