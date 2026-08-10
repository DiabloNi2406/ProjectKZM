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


        // =========================================================
        // DBSET
        // =========================================================

        public DbSet<VaiTro> VaiTros { get; set; }

        public DbSet<NguoiDung> NguoiDungs { get; set; }

        public DbSet<LoaiSach> LoaiSachs { get; set; }

        public DbSet<TheLoai> TheLoais { get; set; }

        public DbSet<Sach> Sachs { get; set; }

        public DbSet<SachTheLoai> SachTheLoais { get; set; }

        public DbSet<GioHang> GioHangs { get; set; }

        public DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }

        public DbSet<DonHang> DonHangs { get; set; }

        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // THE LOAI
            // ==========================================

            modelBuilder.Entity<TheLoai>()
                .HasKey(x => x.MaTheLoai);


            // ==========================================
            // LOAI SACH
            // ==========================================

            modelBuilder.Entity<LoaiSach>()
                .HasKey(x => x.MaLoaiSach);


            // ==========================================
            // LOAI SACH -> SACH
            // ==========================================

            modelBuilder.Entity<Sach>()
                .HasOne(x => x.LoaiSach)
                .WithMany(x => x.Sachs)
                .HasForeignKey(x => x.MaLoaiSach)
                .OnDelete(DeleteBehavior.SetNull);


            // ==========================================
            // SACH <-> THE LOAI
            // ==========================================

            modelBuilder.Entity<SachTheLoai>()
                .HasKey(x => new
                {
                    x.MaSach,
                    x.MaTheLoai
                });


            modelBuilder.Entity<SachTheLoai>()
                .HasOne(x => x.Sach)
                .WithMany(x => x.SachTheLoais)
                .HasForeignKey(x => x.MaSach)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<SachTheLoai>()
                .HasOne(x => x.TheLoai)
                .WithMany(x => x.SachTheLoais)
                .HasForeignKey(x => x.MaTheLoai)
                .OnDelete(DeleteBehavior.Cascade);
            // =====================================================
            // CHI TIET GIO HANG
            // =====================================================

            modelBuilder.Entity<ChiTietGioHang>()
                .HasKey(x => new
                {
                    x.MaGioHang,
                    x.MaSach
                });


            modelBuilder.Entity<ChiTietGioHang>()
                .HasOne(x => x.GioHang)
                .WithMany(x => x.ChiTietGioHangs)
                .HasForeignKey(x => x.MaGioHang);


            modelBuilder.Entity<ChiTietGioHang>()
                .HasOne(x => x.Sach)
                .WithMany(x => x.ChiTietGioHangs)
                .HasForeignKey(x => x.MaSach);


            // =====================================================
            // CHI TIET DON HANG
            // =====================================================

            modelBuilder.Entity<ChiTietDonHang>()
                .HasKey(x => new
                {
                    x.MaDonHang,
                    x.MaSach
                });


            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(x => x.DonHang)
                .WithMany(x => x.ChiTietDonHangs)
                .HasForeignKey(x => x.MaDonHang);


            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(x => x.Sach)
                .WithMany(x => x.ChiTietDonHangs)
                .HasForeignKey(x => x.MaSach);


            // =====================================================
            // NGUOI DUNG -> VAI TRO
            // =====================================================

            modelBuilder.Entity<NguoiDung>()
                .HasOne(x => x.VaiTro)
                .WithMany(x => x.NguoiDungs)
                .HasForeignKey(x => x.MaVaiTro)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // SEED VAI TRO
            // =====================================================

            modelBuilder.Entity<VaiTro>().HasData(

                new VaiTro
                {
                    MaVaiTro = 1,
                    TenVaiTro = "Admin"
                },

                new VaiTro
                {
                    MaVaiTro = 2,
                    TenVaiTro = "User"
                }

            );
        }
    }
}