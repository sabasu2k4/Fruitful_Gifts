using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Fruitful_Gifts.Database;

public partial class FruitfulGiftsschuanContext : DbContext
{
    public FruitfulGiftsschuanContext()
    {
    }

    public FruitfulGiftsschuanContext(DbContextOptions<FruitfulGiftsschuanContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BaiViet> BaiViets { get; set; }

    public virtual DbSet<BinhLuan> BinhLuans { get; set; }

    public virtual DbSet<ChatLieu> ChatLieus { get; set; }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

    public virtual DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }

    public virtual DbSet<ChiTietNhapHang> ChiTietNhapHangs { get; set; }

    public virtual DbSet<ChiTietSanPham> ChiTietSanPhams { get; set; }

    public virtual DbSet<DanhMuc> DanhMucs { get; set; }

    public virtual DbSet<DichVuCongTy> DichVuCongTies { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<DonViTinh> DonViTinhs { get; set; }

    public virtual DbSet<Footer> Footers { get; set; }

    public virtual DbSet<Header> Headers { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<KhoHang> KhoHangs { get; set; }

    public virtual DbSet<KhuyenMai> KhuyenMais { get; set; }

    public virtual DbSet<LienHe> LienHes { get; set; }

    public virtual DbSet<LoaiSanPham> LoaiSanPhams { get; set; }

    public virtual DbSet<NhaCungCap> NhaCungCaps { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<NhapHang> NhapHangs { get; set; }

    public virtual DbSet<PhuongThucThanhToan> PhuongThucThanhToans { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    public virtual DbSet<SanPhamYeuThich> SanPhamYeuThiches { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<XuatXu> XuatXus { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=FRUITFUL_GIFTSSChuan;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaiViet>(entity =>
        {
            entity.HasKey(e => e.MaBv).HasName("PK__BaiViet__272475F5D1003CDE");

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
            entity.HasKey(e => e.IdBinhLuan).HasName("PK__BinhLuan__A6E39FBFEC35839A");

            entity.ToTable("BinhLuan");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Ngay)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TrangThai).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.BinhLuans)
                .HasForeignKey(d => d.MaKh)
                .HasConstraintName("FK_BinhLuan_KhachHang");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.BinhLuans)
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK_BinhLuan_SanPham");
        });

        modelBuilder.Entity<ChatLieu>(entity =>
        {
            entity.HasKey(e => e.MaChatLieu).HasName("PK__ChatLieu__453995BC66E41B72");

            entity.ToTable("ChatLieu");

            entity.Property(e => e.TenChatLieu).HasMaxLength(255);
        });

        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => new { e.MaDh, e.MaSp }).HasName("PK__ChiTietD__F557D6C6D0C39C7A");

            entity.ToTable("ChiTietDonHang");

            entity.Property(e => e.TongTienTungSanPham).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaDhNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaDh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietDonHang_DonHang");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaSp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietDonHang_SanPham");
        });

        modelBuilder.Entity<ChiTietGioHang>(entity =>
        {
            entity.HasKey(e => new { e.MaKh, e.MaSp }).HasName("PK__ChiTietG__F5579FF9229FC1BA");

            entity.ToTable("ChiTietGioHang");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.MaKh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietGioHang_KhachHang");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.MaSp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietGioHang_SanPham");
        });

        modelBuilder.Entity<ChiTietNhapHang>(entity =>
        {
            entity.HasKey(e => new { e.MaNhap, e.MaSp }).HasName("PK__ChiTietN__F13D5F1FFC7E7024");

            entity.ToTable("ChiTietNhapHang");

            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaNhapNavigation).WithMany(p => p.ChiTietNhapHangs)
                .HasForeignKey(d => d.MaNhap)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietNhapHang_NhapHang");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.ChiTietNhapHangs)
                .HasForeignKey(d => d.MaSp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietNhapHang_SanPham");
        });

        modelBuilder.Entity<ChiTietSanPham>(entity =>
        {
            entity.HasKey(e => new { e.MaSpCha, e.MaSpCon }).HasName("PK__ChiTietS__224EB7B45245F680");

            entity.ToTable("ChiTietSanPham");

            entity.HasOne(d => d.MaSpChaNavigation).WithMany(p => p.ChiTietSanPhamMaSpChaNavigations)
                .HasForeignKey(d => d.MaSpCha)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietSanPham_SpCha");

            entity.HasOne(d => d.MaSpConNavigation).WithMany(p => p.ChiTietSanPhamMaSpConNavigations)
                .HasForeignKey(d => d.MaSpCon)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietSanPham_SpCon");
        });

        modelBuilder.Entity<DanhMuc>(entity =>
        {
            entity.HasKey(e => e.MaDm).HasName("PK__DanhMuc__2725864E35436427");

            entity.ToTable("DanhMuc");

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

        modelBuilder.Entity<DichVuCongTy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DichVuCo__3214EC076A937F35");

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
            entity.HasKey(e => e.MaDh).HasName("PK__DonHang__2725864194736B72");

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
            entity.Property(e => e.TrangThai).HasDefaultValue(0);
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
        });

        modelBuilder.Entity<DonViTinh>(entity =>
        {
            entity.HasKey(e => e.MaDvt).HasName("PK__DonViTin__3D824D161D2114C1");

            entity.ToTable("DonViTinh");

            entity.Property(e => e.TenDvt).HasMaxLength(50);
        });

        modelBuilder.Entity<Footer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Footer__3214EC07D958389E");

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

        modelBuilder.Entity<Header>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Header__3214EC073533F750");

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
            entity.HasKey(e => e.MaKh).HasName("PK__KhachHan__2725CF7E8F519C67");

            entity.ToTable("KhachHang");

            entity.HasIndex(e => e.TaiKhoanId, "UQ__KhachHan__9A124B44140423A7").IsUnique();

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
            entity.HasKey(e => e.MaKho).HasName("PK__KhoHang__3BDA9350E19A581D");

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
            entity.HasKey(e => e.Id).HasName("PK__KhuyenMa__3214EC07E28290D1");

            entity.ToTable("KhuyenMai");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MucGiamGia).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TrangThai).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.KhuyenMais)
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK_KhuyenMai_SanPham");
        });

        modelBuilder.Entity<LienHe>(entity =>
        {
            entity.HasKey(e => e.MaLh).HasName("PK__LienHe__2725C75FAA6A00A3");

            entity.ToTable("LienHe");

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.HoTen).HasMaxLength(255);
            entity.Property(e => e.Sdt).HasMaxLength(20);
            entity.Property(e => e.ThoiGianGui)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<LoaiSanPham>(entity =>
        {
            entity.HasKey(e => e.MaLoai).HasName("PK__LoaiSanP__730A5759FF512B66");

            entity.ToTable("LoaiSanPham");

            entity.Property(e => e.TenLoai).HasMaxLength(255);
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.MaNcc).HasName("PK__NhaCungC__3A1951E38462FC1D");

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
            entity.HasKey(e => e.MaNv).HasName("PK__NhanVien__2725D76A83DB1299");

            entity.ToTable("NhanVien");

            entity.HasIndex(e => e.TaiKhoanId, "UQ__NhanVien__9A124B44ED8DB443").IsUnique();

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
            entity.HasKey(e => e.MaNhap).HasName("PK__NhapHang__234F0F98583D23D0");

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
            entity.HasKey(e => e.MaPt).HasName("PK__PhuongTh__2725E7D6CB9D11F1");

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
            entity.HasKey(e => e.MaSp).HasName("PK__SanPham__2725087C97BB24FE");

            entity.ToTable("SanPham");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Gia).HasColumnType("decimal(18, 2)");
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

            entity.HasOne(d => d.MaDmNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.MaDm)
                .HasConstraintName("FK_SanPham_DanhMuc");

            entity.HasOne(d => d.MaDvtNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.MaDvt)
                .HasConstraintName("FK_SanPham_DonViTinh");

            entity.HasOne(d => d.MaLoaiNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.MaLoai)
                .HasConstraintName("FK_SanPham_LoaiSanPham");

            entity.HasOne(d => d.MaNccNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.MaNcc)
                .HasConstraintName("FK_SanPham_NhaCungCap");

            entity.HasOne(d => d.MaXuatXuNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.MaXuatXu)
                .HasConstraintName("FK_SanPham_XuatXu");
        });

        modelBuilder.Entity<SanPhamYeuThich>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SanPhamY__3214EC07163D42AD");

            entity.ToTable("SanPhamYeuThich");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.SanPhamYeuThiches)
                .HasForeignKey(d => d.MaKh)
                .HasConstraintName("FK_SanPhamYeuThich_KhachHang");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.SanPhamYeuThiches)
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK_SanPhamYeuThich_SanPham");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.TaiKhoanId).HasName("PK__TaiKhoan__9A124B4561E13B25");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.TenDangNhap, "UQ__TaiKhoan__55F68FC09A5F6194").IsUnique();

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

        modelBuilder.Entity<XuatXu>(entity =>
        {
            entity.HasKey(e => e.MaXuatXu).HasName("PK__XuatXu__27BB6B193D7889AE");

            entity.ToTable("XuatXu");

            entity.Property(e => e.TenNuoc).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
