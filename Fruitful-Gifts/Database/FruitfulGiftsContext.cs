
﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Fruitful_Gifts.Database;

public partial class FruitfulGiftsContext : DbContext
{
    public FruitfulGiftsContext()
    {
    }

    public FruitfulGiftsContext(DbContextOptions<FruitfulGiftsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BaiViet> BaiViets { get; set; }

    public virtual DbSet<BinhLuan> BinhLuans { get; set; }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

    public virtual DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }

    public virtual DbSet<DanhMuc> DanhMucs { get; set; }

    public virtual DbSet<DichVuCongTy> DichVuCongTies { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<Footer> Footers { get; set; }

    public virtual DbSet<Header> Headers { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<KhuyenMai> KhuyenMais { get; set; }

    public virtual DbSet<LienHe> LienHes { get; set; }

    public virtual DbSet<NhaCungCap> NhaCungCaps { get; set; }

    public virtual DbSet<PhuongThucThanhToan> PhuongThucThanhToans { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    public virtual DbSet<SanPhamYeuThich> SanPhamYeuThiches { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=FRUITFUL_GIFTS;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaiViet>(entity =>
        {
            entity.HasKey(e => e.MaBv).HasName("PK__BaiViet__272475F553DD270B");

            entity.ToTable("BaiViet");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.HinhAnh).HasMaxLength(255);
            entity.Property(e => e.IsHienThi).HasDefaultValue(true);
            entity.Property(e => e.NgayDang).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TieuDe).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<BinhLuan>(entity =>
        {
            entity.HasKey(e => e.IdBinhLuan).HasName("PK__BinhLuan__A6E39FBF6B98036D");

            entity.ToTable("BinhLuan");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsHienThi).HasDefaultValue(true);
            entity.Property(e => e.Ngay)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.BinhLuans)
                .HasForeignKey(d => d.MaKh)
                .HasConstraintName("FK__BinhLuan__MaKh__01142BA1");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.BinhLuans)
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK__BinhLuan__MaSp__00200768");
        });

        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => new { e.MaDh, e.MaSp }).HasName("PK__ChiTietD__F557D6C61917DD32");

            entity.ToTable("ChiTietDonHang");

            entity.Property(e => e.TongTienTungSanPham).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaDhNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaDh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDon__MaDh__73BA3083");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaSp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDon__MaSp__74AE54BC");
        });

        modelBuilder.Entity<ChiTietGioHang>(entity =>
        {
            entity.HasKey(e => new { e.MaKh, e.MaSp }).HasName("PK__ChiTietG__F5579FF9561C28F8");

            entity.ToTable("ChiTietGioHang");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.MaKh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietGio__MaKh__778AC167");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.MaSp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietGio__MaSp__787EE5A0");
        });

        modelBuilder.Entity<DanhMuc>(entity =>
        {
            entity.HasKey(e => e.MaDm).HasName("PK__DanhMuc__2725864E02E67AC7");

            entity.ToTable("DanhMuc");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.HinhAnh).HasMaxLength(255);
            entity.Property(e => e.TenDm).HasMaxLength(255);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<DichVuCongTy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DichVuCo__3214EC07DFDCE2F2");

            entity.ToTable("DichVuCongTy");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.HinhAnh).HasMaxLength(255);
            entity.Property(e => e.IsHienThi).HasDefaultValue(true);
            entity.Property(e => e.TieuDe).HasMaxLength(255);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.MaDh).HasName("PK__DonHang__272586412F38843A");

            entity.ToTable("DonHang");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayDatHang)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PhuongThucBan).HasMaxLength(100);
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);
            entity.Property(e => e.TongTienDonHang).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai).HasDefaultValue(0);
            entity.Property(e => e.TrangThaiThanhToan).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaKh)
                .HasConstraintName("FK__DonHang__MaKh__6B24EA82");

            entity.HasOne(d => d.MaPtNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaPt)
                .HasConstraintName("FK__DonHang__MaPt__6EF57B66");
        });

        modelBuilder.Entity<Footer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Footer__3214EC074BAC541A");

            entity.ToTable("Footer");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DuongLienKet).HasMaxLength(500);
            entity.Property(e => e.HinhAnh).HasMaxLength(255);
            entity.Property(e => e.IsHienThi).HasDefaultValue(true);
            entity.Property(e => e.TieuDe).HasMaxLength(255);
            entity.Property(e => e.TrangThaiHienThi).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Header>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Header__3214EC07990D1915");

            entity.ToTable("Header");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DuongLienKet).HasMaxLength(500);
            entity.Property(e => e.Icon).HasMaxLength(255);
            entity.Property(e => e.IsHienThi).HasDefaultValue(true);
            entity.Property(e => e.TieuDe).HasMaxLength(255);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKh).HasName("PK__KhachHan__2725CF7E7D7457F9");

            entity.ToTable("KhachHang");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.GioiTinh).HasMaxLength(10);
            entity.Property(e => e.HoKh).HasMaxLength(100);
            entity.Property(e => e.MatKhau).HasMaxLength(255);
            entity.Property(e => e.Sdt).HasMaxLength(20);
            entity.Property(e => e.TenKh).HasMaxLength(100);
            entity.Property(e => e.TenNguoiDung).HasMaxLength(100);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<KhuyenMai>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KhuyenMa__3214EC07C070C7B5");

            entity.ToTable("KhuyenMai");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsHienThi).HasDefaultValue(true);
            entity.Property(e => e.MucGiamGia).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.KhuyenMais)
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK__KhuyenMai__MaSp__08B54D69");
        });

        modelBuilder.Entity<LienHe>(entity =>
        {
            entity.HasKey(e => e.MaLh).HasName("PK__LienHe__2725C75F231B9580");

            entity.ToTable("LienHe");

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.HoTen).HasMaxLength(255);
            entity.Property(e => e.Sdt).HasMaxLength(20);
            entity.Property(e => e.ThoiGianGui)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.MaNcc).HasName("PK__NhaCungC__3A1951E3A0D8381B");

            entity.ToTable("NhaCungCap");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Sdt).HasMaxLength(20);
            entity.Property(e => e.TenNcc).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<PhuongThucThanhToan>(entity =>
        {
            entity.HasKey(e => e.MaPt).HasName("PK__PhuongTh__2725E7D648AD2B18");

            entity.ToTable("PhuongThucThanhToan");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TenPt).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.MaSp).HasName("PK__SanPham__2725087C130578BA");

            entity.ToTable("SanPham");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Gia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HinhAnh).HasMaxLength(255);
            entity.Property(e => e.IsHienThi).HasDefaultValue(true);
            entity.Property(e => e.Slug).HasMaxLength(255);
            entity.Property(e => e.TenSp).HasMaxLength(255);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaDmNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.MaDm)
                .HasConstraintName("FK__SanPham__MaDm__5BE2A6F2");

            entity.HasOne(d => d.MaNccNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.MaNcc)
                .HasConstraintName("FK__SanPham__MaNcc__5CD6CB2B");
        });

        modelBuilder.Entity<SanPhamYeuThich>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SanPhamY__3214EC0711301DEA");

            entity.ToTable("SanPhamYeuThich");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.SanPhamYeuThiches)
                .HasForeignKey(d => d.MaKh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SanPhamYeu__MaKh__7C4F7684");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.SanPhamYeuThiches)
                .HasForeignKey(d => d.MaSp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SanPhamYeu__MaSp__7D439ABD");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

