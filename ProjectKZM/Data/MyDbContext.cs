using Microsoft.EntityFrameworkCore;
using ProjectKZM.Models;

namespace ProjectKZM.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {
        }

        // DbSet
        public DbSet<VaiTro> VaiTros { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<TheLoai> TheLoais { get; set; }
        public DbSet<Sach> Sachs { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Khóa chính kép
            modelBuilder.Entity<ChiTietGioHang>()
                .HasKey(ct => new { ct.MaGioHang, ct.MaSach });

            modelBuilder.Entity<ChiTietDonHang>()
                .HasKey(ct => new { ct.MaDonHang, ct.MaSach });

            // Quan hệ ChiTietGioHang
            modelBuilder.Entity<ChiTietGioHang>()
                .HasOne(ct => ct.GioHang)
                .WithMany(g => g.ChiTietGioHangs)
                .HasForeignKey(ct => ct.MaGioHang);

            modelBuilder.Entity<ChiTietGioHang>()
                .HasOne(ct => ct.Sach)
                .WithMany(s => s.ChiTietGioHangs)
                .HasForeignKey(ct => ct.MaSach);

            // Quan hệ ChiTietDonHang
            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.DonHang)
                .WithMany(d => d.ChiTietDonHangs)
                .HasForeignKey(ct => ct.MaDonHang);

            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.Sach)
                .WithMany(s => s.ChiTietDonHangs)
                .HasForeignKey(ct => ct.MaSach);
        }
    }
}
