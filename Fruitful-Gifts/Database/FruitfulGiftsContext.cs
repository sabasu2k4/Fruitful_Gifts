using System;
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

    public virtual DbSet<ChatLieu> ChatLieus { get; set; }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

    public virtual DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }

    public virtual DbSet<ChiTietGioQua> ChiTietGioQuas { get; set; }

    public virtual DbSet<ChiTietNhapHang> ChiTietNhapHangs { get; set; }

    public virtual DbSet<DanhMucGioQua> DanhMucGioQuas { get; set; }

    public virtual DbSet<DanhMucSanPham> DanhMucSanPhams { get; set; }

    public virtual DbSet<DichVuCongTy> DichVuCongTies { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<DonViTinh> DonViTinhs { get; set; }

    public virtual DbSet<Footer> Footers { get; set; }

    public virtual DbSet<GioQua> GioQuas { get; set; }

    public virtual DbSet<Header> Headers { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<KhoHang> KhoHangs { get; set; }

    public virtual DbSet<KhuyenMai> KhuyenMais { get; set; }

    public virtual DbSet<LienHe> LienHes { get; set; }

    public virtual DbSet<Luong> Luongs { get; set; }

    public virtual DbSet<NhaCungCap> NhaCungCaps { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<NhapHang> NhapHangs { get; set; }

    public virtual DbSet<PhuongThucThanhToan> PhuongThucThanhToans { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    public virtual DbSet<SanPhamYeuThich> SanPhamYeuThiches { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<ThanhToan> ThanhToans { get; set; }

    public virtual DbSet<TrangThaiDonHang> TrangThaiDonHangs { get; set; }

    public virtual DbSet<XuatXu> XuatXus { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=ACERNITRO5;Initial Catalog=FRUITFUL_GIFTS;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaiViet>(entity =>
        {
            entity.HasKey(e => e.MaBv).HasName("PK__BaiViet__272475F53633CFD3");

            entity.ToTable("BaiViet");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.HinhAnh).HasMaxLength(255);
            entity.Property(e => e.NgayDang).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Slug).HasMaxLength(255);
            entity.Property(e => e.TieuDe).HasMaxLength(255);
            entity.Property(e => e.TrangThai).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<BinhLuan>(entity =>
        {
            entity.HasKey(e => e.IdBinhLuan).HasName("PK__BinhLuan__A6E39FBFC11E4CB2");

            entity.ToTable("BinhLuan");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayBinhLuan)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TrangThai).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaDhNavigation).WithMany(p => p.BinhLuans)
                .HasForeignKey(d => d.MaDh)
                .HasConstraintName("FK_BinhLuan_DonHang");

            entity.HasOne(d => d.MaGqNavigation).WithMany(p => p.BinhLuans)
                .HasForeignKey(d => d.MaGq)
                .HasConstraintName("FK_BinhLuan_GioQua");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.BinhLuans)
                .HasForeignKey(d => d.MaKh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BinhLuan_KhachHang");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.BinhLuans)
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK_BinhLuan_SanPham");
        });

        modelBuilder.Entity<ChatLieu>(entity =>
        {
            entity.HasKey(e => e.MaChatLieu).HasName("PK__ChatLieu__453995BCBF027B10");

            entity.ToTable("ChatLieu");

            entity.Property(e => e.TenChatLieu).HasMaxLength(255);
        });

        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChiTietD__3214EC07CEEBE918");

            entity.ToTable("ChiTietDonHang");

            entity.Property(e => e.GiaBan).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SoLuong).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TongTienTungSanPham).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaDhNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaDh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietDonHang_DonHang");

            entity.HasOne(d => d.MaGqNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaGq)
                .HasConstraintName("FK_ChiTietDonHang_GioQua");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK_ChiTietDonHang_SanPham");
        });

        modelBuilder.Entity<ChiTietGioHang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChiTietG__3214EC07F4645AD9");

            entity.ToTable("ChiTietGioHang");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaGqNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.MaGq)
                .HasConstraintName("FK_ChiTietGioHang_GioQua");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.MaKh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietGioHang_KhachHang");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK_ChiTietGioHang_SanPham");
        });

        modelBuilder.Entity<ChiTietGioQua>(entity =>
        {
            entity.HasKey(e => new { e.MaGq, e.MaSp }).HasName("PK__ChiTietG__F557FE6B19A72D44");

            entity.ToTable("ChiTietGioQua");

            entity.HasOne(d => d.MaGqNavigation).WithMany(p => p.ChiTietGioQuas)
                .HasForeignKey(d => d.MaGq)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietGioQua_GioQua");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.ChiTietGioQuas)
                .HasForeignKey(d => d.MaSp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietGioQua_SanPham");
        });

        modelBuilder.Entity<ChiTietNhapHang>(entity =>
        {
            entity.HasKey(e => new { e.MaNhap, e.MaSp }).HasName("PK__ChiTietN__F13D5F1F4E4E9482");

            entity.ToTable("ChiTietNhapHang");

            entity.Property(e => e.GiaNhap).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaNhapNavigation).WithMany(p => p.ChiTietNhapHangs)
                .HasForeignKey(d => d.MaNhap)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietNhapHang_NhapHang");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.ChiTietNhapHangs)
                .HasForeignKey(d => d.MaSp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietNhapHang_SanPham");
        });

        modelBuilder.Entity<DanhMucGioQua>(entity =>
        {
            entity.HasKey(e => e.MaDm).HasName("PK__DanhMucG__2725864E26AB7AC5");

            entity.ToTable("DanhMucGioQua");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.HinhAnh).HasMaxLength(255);
            entity.Property(e => e.Slug).HasMaxLength(255);
            entity.Property(e => e.TenDm).HasMaxLength(255);
            entity.Property(e => e.TrangThai).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.DanhMucCha).WithMany(p => p.InverseDanhMucCha)
                .HasForeignKey(d => d.DanhMucChaId)
                .HasConstraintName("FK_DanhMucCha");
        });

        modelBuilder.Entity<DanhMucSanPham>(entity =>
        {
            entity.HasKey(e => e.MaLoai).HasName("PK__DanhMucS__730A575902B5FA5F");

            entity.ToTable("DanhMucSanPham");

            entity.Property(e => e.TenLoai).HasMaxLength(255);
        });

        modelBuilder.Entity<DichVuCongTy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DichVuCo__3214EC0749966967");

            entity.ToTable("DichVuCongTy");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.HinhAnh).HasMaxLength(255);
            entity.Property(e => e.TieuDe).HasMaxLength(255);
            entity.Property(e => e.TrangThai).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.MaDh).HasName("PK__DonHang__2725864141BD7795");

            entity.ToTable("DonHang");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayDatHang)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PhiVanChuyenBanHang)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PhuongThucBan).HasMaxLength(100);
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);
            entity.Property(e => e.TongTienDonHang).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai).HasDefaultValue(1);
            entity.Property(e => e.TrangThaiThanhToan).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaKh)
                .HasConstraintName("FK_DonHang_KhachHang");

            entity.HasOne(d => d.MaNvNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaNv)
                .HasConstraintName("FK_DonHang_NhanVien");

            entity.HasOne(d => d.MaPtNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaPt)
                .HasConstraintName("FK_DonHang_PhuongThucThanhToan");

            entity.HasOne(d => d.TrangThaiNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.TrangThai)
                .HasConstraintName("FK_DonHang_TrangThaiDonHang");
        });

        modelBuilder.Entity<DonViTinh>(entity =>
        {
            entity.HasKey(e => e.MaDvt).HasName("PK__DonViTin__3D824D16FA834041");

            entity.ToTable("DonViTinh");

            entity.Property(e => e.TenDvt).HasMaxLength(50);
        });

        modelBuilder.Entity<Footer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Footer__3214EC0769FF6791");

            entity.ToTable("Footer");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DuongLienKet).HasMaxLength(500);
            entity.Property(e => e.HinhAnh).HasMaxLength(255);
            entity.Property(e => e.TieuDe).HasMaxLength(255);
            entity.Property(e => e.TrangThai).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<GioQua>(entity =>
        {
            entity.HasKey(e => e.MaGq).HasName("PK__GioQua__2725AEEC666BC671");

            entity.ToTable("GioQua");

            entity.Property(e => e.GiaBan).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HinhAnh).HasMaxLength(100);
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.Slug).HasMaxLength(255);
            entity.Property(e => e.TenGioQua).HasMaxLength(100);
            entity.Property(e => e.TrangThai).HasDefaultValue(1);

            entity.HasOne(d => d.MaDmNavigation).WithMany(p => p.GioQuas)
                .HasForeignKey(d => d.MaDm)
                .HasConstraintName("FK_GioQua_DanhMucGioQua");
        });

        modelBuilder.Entity<Header>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Header__3214EC07EDE3D9EA");

            entity.ToTable("Header");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DuongLienKet).HasMaxLength(500);
            entity.Property(e => e.Icon).HasMaxLength(255);
            entity.Property(e => e.TieuDe).HasMaxLength(255);
            entity.Property(e => e.TrangThai).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKh).HasName("PK__KhachHan__2725CF7ED774E1C4");

            entity.ToTable("KhachHang");

            entity.HasIndex(e => e.TaiKhoanId, "UQ__KhachHan__9A124B44A5A3A934").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.GioiTinh).HasMaxLength(10);
            entity.Property(e => e.HoKh).HasMaxLength(50);
            entity.Property(e => e.Sdt).HasMaxLength(20);
            entity.Property(e => e.TenKh).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.TaiKhoan).WithOne(p => p.KhachHang)
                .HasForeignKey<KhachHang>(d => d.TaiKhoanId)
                .HasConstraintName("FK_KhachHang_TaiKhoan");
        });

        modelBuilder.Entity<KhoHang>(entity =>
        {
            entity.HasKey(e => e.MaKho).HasName("PK__KhoHang__3BDA9350F91EB9F6");

            entity.ToTable("KhoHang");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DonViTinh).HasMaxLength(50);
            entity.Property(e => e.TrangThai).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.KhoHangs)
                .HasForeignKey(d => d.MaSp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KhoHang_SanPham");
        });

        modelBuilder.Entity<KhuyenMai>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KhuyenMa__3214EC070EB5E2FC");

            entity.ToTable("KhuyenMai");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MucGiamGia).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TrangThai).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaGqNavigation).WithMany(p => p.KhuyenMais)
                .HasForeignKey(d => d.MaGq)
                .HasConstraintName("FK_KhuyenMai_GioQua");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.KhuyenMais)
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK_KhuyenMai_SanPham");
        });

        modelBuilder.Entity<LienHe>(entity =>
        {
            entity.HasKey(e => e.MaLh).HasName("PK__LienHe__2725C75F1DE54E0E");

            entity.ToTable("LienHe");

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.HoTen).HasMaxLength(255);
            entity.Property(e => e.Sdt).HasMaxLength(20);
            entity.Property(e => e.ThoiGianGui)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<Luong>(entity =>
        {
            entity.HasKey(e => e.MaLuong).HasName("PK__Luong__6609A48D8B33814E");

            entity.ToTable("Luong");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LuongCoBan).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LuongPhuCap)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Phat)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Thuong)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TongLuong)
                .HasComputedColumnSql("(((([LuongCoBan]*[SoNgayCong])/(26)+[LuongPhuCap])+[Thuong])-[Phat])", true)
                .HasColumnType("decimal(36, 6)");

            entity.HasOne(d => d.NhanVien).WithMany(p => p.Luongs)
                .HasForeignKey(d => d.NhanVienId)
                .HasConstraintName("FK_Luong_NhanVien");
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.MaNcc).HasName("PK__NhaCungC__3A1951E3AE3A6457");

            entity.ToTable("NhaCungCap");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Sdt).HasMaxLength(20);
            entity.Property(e => e.TenNcc).HasMaxLength(255);
            entity.Property(e => e.TrangThai).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.MaNv).HasName("PK__NhanVien__2725D76AA2931436");

            entity.ToTable("NhanVien");

            entity.HasIndex(e => e.TaiKhoanId, "UQ__NhanVien__9A124B446DC977AC").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Sdt).HasMaxLength(20);
            entity.Property(e => e.TenNv).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.TaiKhoan).WithOne(p => p.NhanVien)
                .HasForeignKey<NhanVien>(d => d.TaiKhoanId)
                .HasConstraintName("FK_NhanVien_TaiKhoan");
        });

        modelBuilder.Entity<NhapHang>(entity =>
        {
            entity.HasKey(e => e.MaNhap).HasName("PK__NhapHang__234F0F989E8D1B32");

            entity.ToTable("NhapHang");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayNhap)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PhiVanChuyenNhapHang)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaNccNavigation).WithMany(p => p.NhapHangs)
                .HasForeignKey(d => d.MaNcc)
                .HasConstraintName("FK_NhapHang_NhaCungCap");
        });

        modelBuilder.Entity<PhuongThucThanhToan>(entity =>
        {
            entity.HasKey(e => e.MaPt).HasName("PK__PhuongTh__2725E7D6CF51CB13");

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
            entity.HasKey(e => e.MaSp).HasName("PK__SanPham__2725087C6EC5889D");

            entity.ToTable("SanPham");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.GiaBan).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HinhAnh).HasMaxLength(255);
            entity.Property(e => e.Slug).HasMaxLength(255);
            entity.Property(e => e.TenSp).HasMaxLength(255);
            entity.Property(e => e.TrangThai).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaChatLieuNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.MaChatLieu)
                .HasConstraintName("FK_SanPham_ChatLieu");

            entity.HasOne(d => d.MaDvtNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.MaDvt)
                .HasConstraintName("FK_SanPham_DonViTinh");

            entity.HasOne(d => d.MaLoaiNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.MaLoai)
                .HasConstraintName("FK_SanPham_DanhMucSanPham");

            entity.HasOne(d => d.MaNccNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.MaNcc)
                .HasConstraintName("FK_SanPham_NhaCungCap");

            entity.HasOne(d => d.MaXuatXuNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.MaXuatXu)
                .HasConstraintName("FK_SanPham_XuatXu");
        });

        modelBuilder.Entity<SanPhamYeuThich>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SanPhamY__3214EC070CAC138E");

            entity.ToTable("SanPhamYeuThich");

            entity.HasOne(d => d.MaGqNavigation).WithMany(p => p.SanPhamYeuThiches)
                .HasForeignKey(d => d.MaGq)
                .HasConstraintName("FK_SanPhamYeuThich_GioQua");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.SanPhamYeuThiches)
                .HasForeignKey(d => d.MaKh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SanPhamYeuThich_KhachHang");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.SanPhamYeuThiches)
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK_SanPhamYeuThich_SanPham");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.TaiKhoanId).HasName("PK__TaiKhoan__9A124B453DDEDB6B");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.TenDangNhap, "UQ__TaiKhoan__55F68FC0B057E337").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MatKhau).HasMaxLength(255);
            entity.Property(e => e.TenDangNhap).HasMaxLength(255);
            entity.Property(e => e.TrangThai).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.VaiTro).HasMaxLength(20);
        });

        modelBuilder.Entity<ThanhToan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ThanhToa__3214EC07ECF41EE6");

            entity.ToTable("ThanhToan");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BankCode).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentStatus).HasMaxLength(20);
            entity.Property(e => e.PaymentTime).HasColumnType("datetime");
            entity.Property(e => e.ResponseCode).HasMaxLength(50);
            entity.Property(e => e.TransactionCode).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.MaDhNavigation).WithMany(p => p.ThanhToans)
                .HasForeignKey(d => d.MaDh)
                .HasConstraintName("FK_Payments_DonHang");

            entity.HasOne(d => d.MaPtNavigation).WithMany(p => p.ThanhToans)
                .HasForeignKey(d => d.MaPt)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_PhuongThucThanhToan");
        });

        modelBuilder.Entity<TrangThaiDonHang>(entity =>
        {
            entity.HasKey(e => e.MaTrangThai).HasName("PK__TrangTha__AADE41387EFEA545");

            entity.ToTable("TrangThaiDonHang");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenTrangThai).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<XuatXu>(entity =>
        {
            entity.HasKey(e => e.MaXuatXu).HasName("PK__XuatXu__27BB6B19DF973762");

            entity.ToTable("XuatXu");

            entity.Property(e => e.TenNuoc).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
