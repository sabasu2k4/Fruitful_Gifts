--CREATE DATABASE FRUITFUL_GIFTS;--GO--USE FRUITFUL_GIFTS;--GO
-- Bảng Header
CREATE TABLE Header (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TieuDe NVARCHAR(255),
    DuongLienKet NVARCHAR(500),
    Icon NVARCHAR(255),
    ViTriHienThi INT,
    TrangThai BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO
-- Bảng Footer
CREATE TABLE Footer (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TieuDe NVARCHAR(255),
    MoTa NVARCHAR(MAX),
    HinhAnh NVARCHAR(255),
    DuongLienKet NVARCHAR(500),
    TrangThaiHienThi BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO
-- Bảng Danh mục
CREATE TABLE DanhMuc (
    MaDm INT PRIMARY KEY IDENTITY(1,1),
    TenDm NVARCHAR(255),
	HinhAnh NVARCHAR(255),
    TrangThai BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO
-- Bảng Nhà cung cấp
CREATE TABLE NhaCungCap (
    MaNcc INT PRIMARY KEY IDENTITY(1,1),
    TenNcc NVARCHAR(255),
    Sdt NVARCHAR(20),
    Email NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO
-- Bảng Sản phẩm
CREATE TABLE SanPham (
    MaSp INT PRIMARY KEY IDENTITY(1,1),
    TenSp NVARCHAR(255),
    MaDm INT FOREIGN KEY REFERENCES DanhMuc(MaDm),
    MaNcc INT FOREIGN KEY REFERENCES NhaCungCap(MaNcc),
    Gia DECIMAL(18,2),
    SoLuong INT,
    MoTa NVARCHAR(MAX),
    Slug NVARCHAR(255),
    TrangThai BIT DEFAULT 1,
    HinhAnh NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO
-- Bảng Phương thức thanh toán
CREATE TABLE PhuongThucThanhToan (
    MaPt INT PRIMARY KEY IDENTITY(1,1),
    TenPt NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO
-- Bảng Khách hàng
CREATE TABLE KhachHang (
    MaKh INT PRIMARY KEY IDENTITY(1,1),
    HoKh NVARCHAR(100),
    TenKh NVARCHAR(100),
    GioiTinh NVARCHAR(10),
    Email NVARCHAR(255),
    Sdt NVARCHAR(20),
    DiaChi NVARCHAR(MAX),
    TenNguoiDung NVARCHAR(100),
    MatKhau NVARCHAR(255),
    TrangThai BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO
-- Bảng Đơn hàng
CREATE TABLE DonHang (
    MaDh INT PRIMARY KEY IDENTITY(1,1),
    MaKh INT FOREIGN KEY REFERENCES KhachHang(MaKh),
    NgayDatHang DATETIME DEFAULT GETDATE(),
    TongTienDonHang DECIMAL(18,2),
    TrangThai INT DEFAULT 0, -- 0: Đang xử lý, 1: Đang giao, 2: Hoàn tất
    TrangThaiThanhToan INT DEFAULT 0, -- 0: Chưa thanh toán, 1: Đã thanh toán
    DiaChiNhanHang NVARCHAR(MAX),
    PhuongThucBan NVARCHAR(100),
    MaPt INT FOREIGN KEY REFERENCES PhuongThucThanhToan(MaPt),
    GhiChu NVARCHAR(MAX),
    SoDienThoai NVARCHAR(20),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO
-- Bảng Chi tiết đơn hàng
CREATE TABLE ChiTietDonHang (
    MaDh INT,
    MaSp INT,
    SoLuong INT,
    TongTienTungSanPham DECIMAL(18,2),
PRIMARY KEY (MaDh, MaSp),
    FOREIGN KEY (MaDh) REFERENCES DonHang(MaDh),
    FOREIGN KEY (MaSp) REFERENCES SanPham(MaSp)
);
GO
-- Bảng Chi tiết giỏ hàng
CREATE TABLE ChiTietGioHang (
    MaKh INT,
    MaSp INT,
    SoLuong INT,
    PRIMARY KEY (MaKh, MaSp),
    FOREIGN KEY (MaKh) REFERENCES KhachHang(MaKh),
    FOREIGN KEY (MaSp) REFERENCES SanPham(MaSp),
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO
-- Bảng Sản phẩm yêu thích
CREATE TABLE SanPhamYeuThich (
    MaKh INT,
    MaSp INT,
    PRIMARY KEY (MaKh, MaSp),
    FOREIGN KEY (MaKh) REFERENCES KhachHang(MaKh),
    FOREIGN KEY (MaSp) REFERENCES SanPham(MaSp)
);
GO
-- Bảng Bình luận (Review)
CREATE TABLE BinhLuan (
    IdBinhLuan INT PRIMARY KEY IDENTITY(1,1),
    MaSp INT FOREIGN KEY REFERENCES SanPham(MaSp),
    MaKh INT FOREIGN KEY REFERENCES KhachHang(MaKh),
    SoSao INT CHECK (SoSao BETWEEN 1 AND 5),
    NoiDung NVARCHAR(MAX),
    Ngay DATETIME DEFAULT GETDATE(),
    TrangThai BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO
-- Bảng Khuyến mãi
CREATE TABLE KhuyenMai (
    Id INT PRIMARY KEY IDENTITY(1,1),
    MaSp INT FOREIGN KEY REFERENCES SanPham(MaSp),
    MucGiamGia DECIMAL(5,2), -- Ví dụ 10.00 tức là giảm 10%
    NgayBatDau DATE,
    NgayKetThuc DATE,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO
-- Bảng Bài viết
CREATE TABLE BaiViet (
    MaBv INT PRIMARY KEY IDENTITY(1,1),
    TieuDe NVARCHAR(255),
    HinhAnh NVARCHAR(255),
    NoiDung NVARCHAR(MAX),
    NgayDang DATE DEFAULT GETDATE(),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO
-- Bảng Liên hệ
CREATE TABLE LienHe (
    MaLh INT PRIMARY KEY IDENTITY(1,1),
    HoTen NVARCHAR(255),
    Email NVARCHAR(255),
    Sdt NVARCHAR(20),
    NoiDung NVARCHAR(MAX),
    ThoiGianGui DATETIME DEFAULT GETDATE(),
    TrangThai BIT DEFAULT 1
);
GO
-- Bảng Dịch vụ công ty
CREATE TABLE DichVuCongTy (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TieuDe NVARCHAR(255),
    MoTa NVARCHAR(MAX),
    HinhAnh NVARCHAR(255),
    TrangThai BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO
ALTER TABLE SanPham ADD IsHienThi BIT DEFAULT 1;
ALTER TABLE BinhLuan ADD IsHienThi BIT DEFAULT 1;
ALTER TABLE BaiViet ADD IsHienThi BIT DEFAULT 1;
ALTER TABLE KhuyenMai ADD IsHienThi BIT DEFAULT 1;
ALTER TABLE DichVuCongTy ADD IsHienThi BIT DEFAULT 1;
ALTER TABLE Footer ADD IsHienThi BIT DEFAULT 1;
ALTER TABLE Header ADD IsHienThi BIT DEFAULT 1;
GO

-- Insert dữ liệu vào bảng Header
INSERT INTO Header (TieuDe, DuongLienKet, Icon, ViTriHienThi, TrangThai, CreatedAt, UpdatedAt)
VALUES 
    (N'Trang chủ', '/', 'home.jpg', 1, 1, GETDATE(), GETDATE()),
    (N'Sản phẩm', '/san-pham', 'products.jpg', 2, 1, GETDATE(), GETDATE()),
    (N'Giới thiệu', '/gioi-thieu', 'about.jpg', 3, 1, GETDATE(), GETDATE()),
    (N'Tin tức', '/tin-tuc', 'news.jpg', 4, 1, GETDATE(), GETDATE()),
    (N'Liên hệ', '/lien-he', 'contact.jpg', 5, 1, GETDATE(), GETDATE()),
    (N'Khuyến mãi', '/khuyen-mai', 'sale.jpg', 6, 1, GETDATE(), GETDATE()),
    (N'Blog', '/blog', 'blog.jpg', 7, 1, GETDATE(), GETDATE()),
    (N'Dịch vụ', '/dich-vu', 'services.jpg', 8, 1, GETDATE(), GETDATE()),
    (N'Cửa hàng', '/cua-hang', 'store.jpg', 9, 1, GETDATE(), GETDATE()),
    (N'Tuyển dụng', '/tuyen-dung', 'jobs.jpg', 10, 1, GETDATE(), GETDATE()),
    (N'Quà tặng 8/3', '/qua-tang-8-3', '8-3-quoc-te-phu-nu.jpg', 11, 1, GETDATE(), GETDATE()),
    (N'Quà tặng 20/10', '/qua-tang-20-10', '20-10-phu-nu-viet-nam.jpg', 12, 1, GETDATE(), GETDATE()),
    (N'Quà tặng sinh nhật', '/qua-tang-sinh-nhat', 'sinh-nhat.jpg', 13, 1, GETDATE(), GETDATE()),
    (N'Quà tặng tốt nghiệp', '/qua-tang-tot-nghiep', 'graduation.jpg', 14, 1, GETDATE(), GETDATE()),
    (N'Quà tặng Valentine', '/qua-tang-valentine', 'valentine.jpg', 15, 1, GETDATE(), GETDATE());
GO

-- Insert dữ liệu vào bảng Footer
INSERT INTO Footer (TieuDe, MoTa, HinhAnh, DuongLienKet, TrangThaiHienThi, CreatedAt, UpdatedAt)
VALUES 
    (N'Về chúng tôi', N'Công ty chuyên cung cấp các sản phẩm quà tặng độc đáo và ý nghĩa.', 'logo.jpg', '/gioi-thieu', 1, GETDATE(), GETDATE()),
    (N'Địa chỉ', N'123 Đường ABC, Quận XYZ, Thành phố Hồ Chí Minh', 'map.jpg', '/lien-he', 1, GETDATE(), GETDATE()),
    (N'Điện thoại', N'0901 234 567', NULL, '/lien-he', 1, GETDATE(), GETDATE()),
    (N'Email', N'info@example.com', NULL, '/lien-he', 1, GETDATE(), GETDATE()),
    (N'Sản phẩm', N'Xem các loại quà tặng của chúng tôi', NULL, '/san-pham', 1, GETDATE(), GETDATE()),
    (N'Tin tức', N'Cập nhật tin tức và sự kiện mới nhất', NULL, '/tin-tuc', 1, GETDATE(), GETDATE()),
    (N'Chính sách bảo mật', N'Chính sách bảo mật thông tin khách hàng', NULL, '/chinh-sach-bao-mat', 1, GETDATE(), GETDATE()),
    (N'Điều khoản dịch vụ', N'Điều khoản và điều kiện sử dụng website', NULL, '/dieu-khoan-dich-vu', 1, GETDATE(), GETDATE()),
    (N'Hướng dẫn mua hàng', N'Hướng dẫn chi tiết các bước đặt hàng', NULL, '/huong-dan-mua-hang', 1, GETDATE(), GETDATE()),
    (N'Phương thức thanh toán', N'Các hình thức thanh toán được chấp nhận', 'payment-methods.jpg', '/phuong-thuc-thanh-toan', 1, GETDATE(), GETDATE()),
    (N'Facebook', N'Kết nối với chúng tôi trên Facebook', 'facebook.jpg', 'https://www.facebook.com', 1, GETDATE(), GETDATE()),
    (N'Instagram', N'Theo dõi chúng tôi trên Instagram', 'instagram.jpg', 'https://www.instagram.com', 1, GETDATE(), GETDATE()),
    (N'Twitter', N'Theo dõi chúng tôi trên Twitter', 'twitter.jpg', 'https://www.twitter.com', 1, GETDATE(), GETDATE()),
    (N'Youtube', N'Xem video của chúng tôi trên Youtube', 'youtube.jpg', 'https://www.youtube.com', 1, GETDATE(), GETDATE()),
    (N'Bản đồ', N'Xem bản đồ đường đi đến cửa hàng', 'map.jpg', '/ban-do', 1, GETDATE(), GETDATE());
GO

-- Insert dữ liệu vào bảng DanhMuc
INSERT INTO DanhMuc (TenDm, HinhAnh, TrangThai, CreatedAt, UpdatedAt)
VALUES 
(N'8/3 Quốc tế Phụ nữ', '8-3-quoc-te-phu-nu.jpg', 1, GETDATE(), GETDATE()),
(N'20/10 Phụ nữ Việt Nam', '20-10-phu-nu-viet-nam.jpg', 1, GETDATE(), GETDATE()),
(N'20/11 Nhà giáo Việt Nam', '20-11-nha-giao-viet-nam.jpg', 1, GETDATE(), GETDATE()),
(N'Khai trương', 'khai-truong.jpg', 1, GETDATE(), GETDATE()),
(N'Sinh nhật', 'sinh-nhat.jpg', 1, GETDATE(), GETDATE()),
(N'Tân gia', 'tan-gia.jpg', 1, GETDATE(), GETDATE()),
(N'Tết Nguyên Đán', 'tet-nguyen-dan.jpg', 1, GETDATE(), GETDATE()),
(N'Valentine', 'valentine.jpg', 1, GETDATE(), GETDATE()),
(N'Cưới hỏi', 'cuoi-hoi.jpg', 1, GETDATE(), GETDATE());

GO

-- Insert dữ liệu vào bảng NhaCungCap
INSERT INTO NhaCungCap (TenNcc, Sdt, Email, CreatedAt, UpdatedAt)
VALUES 
(N'Công ty TNHH Trái Cây Miền Nam', '0909123456', 'lienhe@traicaymiennam.vn', GETDATE(), GETDATE()),
(N'Công ty TNHH Trái Cây 24h', '0911222333', 'contact@traicay24h.vn', GETDATE(), GETDATE()),
(N'Công ty TNHH Bao Bì & Giỏ Quà Minh Tâm', '0933444555', 'baobi@minhtam.com.vn', GETDATE(), GETDATE()),
(N'Cửa Hàng Trái Cây Nhập Khẩu GreenFruit', '0977888999', 'greenfruit@gmail.com', GETDATE(), GETDATE()),
(N'Nhà Phân Phối Trái Cây Ngọc Lan', '0966778899', 'ngoclanfruit@vietmail.vn', GETDATE(), GETDATE());

GO

-- Insert dữ liệu vào bảng SanPham
-- Insert 10 sản phẩm mẫu cho dịp 8/3
INSERT INTO SanPham (TenSp, MaDm, MaNcc, Gia, SoLuong, MoTa, Slug, TrangThai, HinhAnh, CreatedAt, UpdatedAt)
VALUES 
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 8/3 - 01', 1, 1, 450000.00, 20, N'Giỏ quà gồm táo, nho, cam và dâu tươi cao cấp.', 'qua-trai-cay-tuoi-8-3-001', 1, '8-3-quoc-te-phu-nu-001.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 8/3 - 02', 1, 2, 720000.00, 15, N'Sản phẩm gồm kiwi, lê Hàn, nho Mỹ, táo Pháp.', 'qua-trai-cay-tuoi-8-3-002', 1, '8-3-quoc-te-phu-nu-002.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 8/3 - 03', 1, 3, 980000.00, 10, N'Đóng hộp gỗ, trái cây nhập khẩu kèm thiệp chúc.', 'qua-trai-cay-tuoi-8-3-003', 1, '8-3-quoc-te-phu-nu-003.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 8/3 - 04', 1, 1, 650000.00, 12, N'Giỏ gồm trái cây và hoa baby trắng trang trí đẹp mắt.', 'qua-trai-cay-tuoi-8-3-004', 1, '8-3-quoc-te-phu-nu-004.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 8/3 - 05', 1, 2, 500000.00, 18, N'Dành tặng phụ nữ với trái cây tốt cho sức khỏe.', 'qua-trai-cay-tuoi-8-3-005', 1, '8-3-quoc-te-phu-nu-005.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 8/3 - 06', 1, 3, 390000.00, 25, N'Phù hợp làm quà cho bạn nữ đồng nghiệp, trang nhã.', 'qua-trai-cay-tuoi-8-3-006', 1, '8-3-quoc-te-phu-nu-006.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 8/3 - 07', 1, 1, 880000.00, 8, N'Trọn vẹn tình cảm với trái cây và socola nhập.', 'qua-trai-cay-tuoi-8-3-007', 1, '8-3-quoc-te-phu-nu-007.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 8/3 - 08', 1, 2, 550000.00, 20, N'Dễ ăn, nhẹ nhàng, phù hợp mẹ lớn tuổi.', 'qua-trai-cay-tuoi-8-3-008', 1, '8-3-quoc-te-phu-nu-008.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 8/3 - 09', 1, 3, 1100000.00, 5, N'Trái cây tuyển chọn, hộp da sang trọng, có nơ.', 'qua-trai-cay-tuoi-8-3-009', 1, '8-3-quoc-te-phu-nu-009.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 8/3 - 10', 1, 1, 290000.00, 30, N'Giỏ mini xinh xắn, tặng bạn bè, học sinh – sinh viên.', 'qua-trai-cay-tuoi-8-3-010', 1, '8-3-quoc-te-phu-nu-010.jpg', GETDATE(), GETDATE()),

(N'Quà Trái Cây Tươi Tặng Dịp Lễ 20/10 - 01', 2, 1, 480000.00, 20, N'Giỏ quà gồm táo, cam Úc, nho đen, dưa lưới Nhật.', 'qua-trai-cay-tuoi-20-10-001', 1, '20-10-phu-nu-viet-nam-001.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 20/10 - 02', 2, 2, 750000.00, 12, N'Hộp quà gỗ sang trọng gồm trái cây nhập khẩu cao cấp.', 'qua-trai-cay-tuoi-20-10-002', 1, '20-10-phu-nu-viet-nam-002.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 20/10 - 03', 2, 3, 520000.00, 18, N'Giỏ hoa quả và socola tặng mẹ, kèm thiệp chúc.', 'qua-trai-cay-tuoi-20-10-003', 1, '20-10-phu-nu-viet-nam-003.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 20/10 - 04', 2, 1, 430000.00, 25, N'Trọn gói trái cây nhẹ nhàng, dễ ăn, phù hợp mọi lứa tuổi.', 'qua-trai-cay-tuoi-20-10-004', 1, '20-10-phu-nu-viet-nam-004.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 20/10 - 05', 2, 2, 690000.00, 10, N'Giỏ quà trang trí tinh tế, có thể tặng sếp nữ.', 'qua-trai-cay-tuoi-20-10-005', 1, '20-10-phu-nu-viet-nam-005.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 20/10 - 06', 2, 3, 310000.00, 30, N'Phiên bản mini dành tặng bạn bè, học sinh – sinh viên.', 'qua-trai-cay-tuoi-20-10-006', 1, '20-10-phu-nu-viet-nam-006.jpg', GETDATE(), GETDATE()),

(N'Quà Trái Cây Tươi Tặng Dịp Lễ 20/11 - 01', 3, 1, 520000.00, 15, N'Giỏ quà gồm nho Mỹ, táo Envy, lê Hàn và cam Úc.', 'qua-trai-cay-tuoi-20-11-001', 1, '20-11-nha-giao-viet-nam-001.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 20/11 - 02', 3, 2, 670000.00, 10, N'Giỏ trái cây nhập khẩu cao cấp, có nơ, thiệp cảm ơn.', 'qua-trai-cay-tuoi-20-11-002', 1, '20-11-nha-giao-viet-nam-002.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 20/11 - 03', 3, 3, 450000.00, 20, N'Trọn bộ trái cây, hoa và socola sang trọng tặng thầy cô.', 'qua-trai-cay-tuoi-20-11-003', 1, '20-11-nha-giao-viet-nam-003.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 20/11 - 04', 3, 1, 390000.00, 25, N'Hộp gỗ nhỏ xinh, gồm trái cây tươi và thiệp viết tay.', 'qua-trai-cay-tuoi-20-11-004', 1, '20-11-nha-giao-viet-nam-004.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 20/11 - 05', 3, 2, 800000.00, 8, N'Hộp da sang trọng, trái cây nhập khẩu nguyên thùng.', 'qua-trai-cay-tuoi-20-11-005', 1, '20-11-nha-giao-viet-nam-005.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 20/11 - 06', 3, 3, 620000.00, 14, N'Giỏ trái cây cao cấp phù hợp tri ân giáo viên nữ.', 'qua-trai-cay-tuoi-20-11-006', 1, '20-11-nha-giao-viet-nam-006.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 20/11 - 07', 3, 1, 350000.00, 30, N'Mẫu giỏ nhỏ, nhẹ nhàng dành cho học sinh tặng giáo viên.', 'qua-trai-cay-tuoi-20-11-007', 1, '20-11-nha-giao-viet-nam-007.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 20/11 - 08', 3, 2, 710000.00, 12, N'Giỏ trái cây kết hợp trà và socola nhập khẩu.', 'qua-trai-cay-tuoi-20-11-008', 1, '20-11-nha-giao-viet-nam-008.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Lễ 20/11 - 09', 3, 3, 580000.00, 16, N'Trọn bộ quà tinh tế với trái cây, ruy băng và lời chúc.', 'qua-trai-cay-tuoi-20-11-009', 1, '20-11-nha-giao-viet-nam-009.jpg', GETDATE(), GETDATE()),

(N'Quà Trái Cây Tươi Tặng Dịp Khai Trương - 01', 4, 1, 850000.00, 10, N'Giỏ trái cây sang trọng, phối hoa chúc mừng khai trương.', 'qua-trai-cay-tuoi-khai-truong-001', 1, 'khai-truong-001.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Khai Trương - 02', 4, 2, 920000.00, 8, N'Giỏ trái cây nhập khẩu, đi kèm thiệp chúc và hoa tươi.', 'qua-trai-cay-tuoi-khai-truong-002', 1, 'khai-truong-002.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Khai Trương - 03', 4, 3, 780000.00, 12, N'Trọn bộ trái cây, bánh và socola, thích hợp tặng khai trương.', 'qua-trai-cay-tuoi-khai-truong-003', 1, 'khai-truong-003.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Khai Trương - 04', 4, 1, 690000.00, 15, N'Giỏ quà trái cây tươi kèm hoa baby, mang ý nghĩa may mắn.', 'qua-trai-cay-tuoi-khai-truong-004', 1, 'khai-truong-004.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Khai Trương - 05', 4, 2, 950000.00, 6, N'Hộp gỗ trái cây cao cấp, trang trí chỉn chu, lịch sự.', 'qua-trai-cay-tuoi-khai-truong-005', 1, 'khai-truong-005.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Khai Trương - 06', 4, 3, 720000.00, 14, N'Sản phẩm trái cây nhập khẩu được chọn lọc kỹ lưỡng.', 'qua-trai-cay-tuoi-khai-truong-006', 1, 'khai-truong-006.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Khai Trương - 07', 4, 1, 830000.00, 10, N'Giỏ quà phối màu đỏ, vàng - tượng trưng tài lộc, phát đạt.', 'qua-trai-cay-tuoi-khai-truong-007', 1, 'khai-truong-007.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Khai Trương - 08', 4, 2, 890000.00, 9, N'Giỏ trái cây có logo công ty khách hàng - thiết kế riêng.', 'qua-trai-cay-tuoi-khai-truong-008', 1, 'khai-truong-008.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Khai Trương - 09', 4, 3, 680000.00, 20, N'Giỏ quà thiết kế đơn giản nhưng đầy ý nghĩa.', 'qua-trai-cay-tuoi-khai-truong-009', 1, 'khai-truong-009.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Khai Trương - 10', 4, 1, 1100000.00, 5, N'Hộp quà cao cấp, gồm trái cây, trà và thiệp chúc mừng.', 'qua-trai-cay-tuoi-khai-truong-010', 1, 'khai-truong-010.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Khai Trương - 11', 4, 2, 770000.00, 13, N'Mẫu giỏ quà nhẹ nhàng, ý nghĩa, thích hợp mọi ngành nghề.', 'qua-trai-cay-tuoi-khai-truong-011', 1, 'khai-truong-011.jpg', GETDATE(), GETDATE()),

(N'Quà Trái Cây Tươi Tặng Dịp Sinh Nhật - 01', 5, 1, 520000.00, 15, N'Giỏ trái cây nhỏ xinh kèm thiệp sinh nhật dễ thương.', 'qua-trai-cay-tuoi-sinh-nhat-001', 1, 'sinh-nhat-001.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Sinh Nhật - 02', 5, 2, 670000.00, 10, N'Trọn bộ trái cây nhập khẩu và bánh cookie mini.', 'qua-trai-cay-tuoi-sinh-nhat-002', 1, 'sinh-nhat-002.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Sinh Nhật - 03', 5, 3, 750000.00, 8, N'Giỏ quà phối màu pastel, nhẹ nhàng và tinh tế.', 'qua-trai-cay-tuoi-sinh-nhat-003', 1, 'sinh-nhat-003.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Sinh Nhật - 04', 5, 1, 820000.00, 6, N'Hộp quà trái cây tươi kết hợp socola và ruy băng sinh nhật.', 'qua-trai-cay-tuoi-sinh-nhat-004', 1, 'sinh-nhat-004.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Sinh Nhật - 05', 5, 2, 590000.00, 12, N'Sản phẩm dành tặng bạn bè, người thân trong ngày sinh nhật.', 'qua-trai-cay-tuoi-sinh-nhat-005', 1, 'sinh-nhat-005.jpg', GETDATE(), GETDATE()),

(N'Quà Trái Cây Tươi Tặng Dịp Tân Gia - 01', 6, 1, 680000.00, 10, N'Giỏ trái cây trang trọng chúc mừng tân gia.', 'qua-trai-cay-tuoi-tan-gia-001', 1, 'tan-gia-001.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Tân Gia - 02', 6, 2, 750000.00, 8, N'Hộp trái cây cao cấp kèm nơ, chúc phát tài.', 'qua-trai-cay-tuoi-tan-gia-002', 1, 'tan-gia-002.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Tân Gia - 03', 6, 3, 820000.00, 6, N'Sản phẩm gồm nho Mỹ, táo xanh, cam vàng.', 'qua-trai-cay-tuoi-tan-gia-003', 1, 'tan-gia-003.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Tân Gia - 04', 6, 1, 940000.00, 5, N'Hộp quà gỗ trái cây nhập khẩu, sang trọng.', 'qua-trai-cay-tuoi-tan-gia-004', 1, 'tan-gia-004.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Tân Gia - 05', 6, 2, 590000.00, 10, N'Giỏ quà vừa phải với trái cây Việt & nhập.', 'qua-trai-cay-tuoi-tan-gia-005', 1, 'tan-gia-005.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Tân Gia - 06', 6, 3, 780000.00, 7, N'Giỏ quà chúc mừng nhà mới bình an, tài lộc.', 'qua-trai-cay-tuoi-tan-gia-006', 1, 'tan-gia-006.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Tân Gia - 07', 6, 1, 880000.00, 5, N'Trái cây tươi kết hợp hoa tươi trang trí.', 'qua-trai-cay-tuoi-tan-gia-007', 1, 'tan-gia-007.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Tân Gia - 08', 6, 2, 610000.00, 12, N'Giỏ nhỏ xinh dành cho bạn bè, đồng nghiệp.', 'qua-trai-cay-tuoi-tan-gia-008', 1, 'tan-gia-008.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Tân Gia - 09', 6, 3, 990000.00, 4, N'Hộp trái cây mix cùng mật ong, trà thảo mộc.', 'qua-trai-cay-tuoi-tan-gia-009', 1, 'tan-gia-009.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Tân Gia - 10', 6, 1, 720000.00, 9, N'Trái cây sạch được sắp xếp nghệ thuật, bắt mắt.', 'qua-trai-cay-tuoi-tan-gia-010', 1, 'tan-gia-010.jpg', GETDATE(), GETDATE()),

(N'Quà Trái Cây Tươi Tặng Dịp Tết Nguyên Đán - 01', 7, 1, 880000.00, 12, N'Giỏ quà tết trái cây tươi cao cấp, chúc xuân an khang.', 'qua-trai-cay-tuoi-tet-nguyen-dan-001', 1, 'tet-001.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Tết Nguyên Đán - 02', 7, 2, 1020000.00, 8, N'Trái cây nhập khẩu sắp xếp nghệ thuật trong giỏ có nơ.', 'qua-trai-cay-tuoi-tet-nguyen-dan-002', 1, 'tet-002.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Tết Nguyên Đán - 03', 7, 3, 950000.00, 10, N'Giỏ quà sang trọng gồm táo đỏ, nho đen, cam vàng, hoa.', 'qua-trai-cay-tuoi-tet-nguyen-dan-003', 1, 'tet-003.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Tết Nguyên Đán - 04', 7, 1, 680000.00, 15, N'Giỏ trái cây tết cho người thân, ý nghĩa và tinh tế.', 'qua-trai-cay-tuoi-tet-nguyen-dan-004', 1, 'tet-004.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Tết Nguyên Đán - 05', 7, 2, 770000.00, 9, N'Hộp trái cây tết thiết kế hiện đại, lịch sự.', 'qua-trai-cay-tuoi-tet-nguyen-dan-005', 1, 'tet-005.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Tết Nguyên Đán - 06', 7, 3, 590000.00, 20, N'Giỏ quà nhỏ gọn dành tặng đồng nghiệp, bạn bè dịp tết.', 'qua-trai-cay-tuoi-tet-nguyen-dan-006', 1, 'tet-006.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Tết Nguyên Đán - 07', 7, 1, 1130000.00, 6, N'Giỏ quà trái cây tươi loại đặc biệt, gửi gắm lời chúc năm mới.', 'qua-trai-cay-tuoi-tet-nguyen-dan-007', 1, 'tet-007.jpg', GETDATE(), GETDATE()),

(N'Quà Trái Cây Tươi Tặng Dịp Valentine - 01', 8, 1, 690000.00, 15, N'Giỏ trái cây tươi kèm hoa hồng đỏ, món quà tặng ngọt ngào dịp 14/2.', 'qua-trai-cay-tuoi-valentine-001', 1, 'valentine-001.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Valentine - 02', 8, 2, 820000.00, 10, N'Hộp trái cây nhập khẩu kết hợp socola cao cấp, ý nghĩa và tinh tế.', 'qua-trai-cay-tuoi-valentine-002', 1, 'valentine-002.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Valentine - 03', 8, 3, 590000.00, 20, N'Giỏ quà nhỏ xinh gồm dâu, nho đỏ và thiệp chúc ngọt ngào.', 'qua-trai-cay-tuoi-valentine-003', 1, 'valentine-003.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Valentine - 04', 8, 1, 970000.00, 8, N'Trái cây tươi tuyển chọn, đựng trong hộp tim đẹp mắt, dành tặng người yêu.', 'qua-trai-cay-tuoi-valentine-004', 1, 'valentine-004.jpg', GETDATE(), GETDATE()),
(N'Quà Trái Cây Tươi Tặng Dịp Valentine - 05', 8, 2, 760000.00, 12, N'Giỏ quà Valentine phong cách lãng mạn, trái cây tươi, nơ và thiệp.', 'qua-trai-cay-tuoi-valentine-005', 1, 'valentine-005.jpg', GETDATE(), GETDATE());

GO

-- Insert dữ liệu vào bảng PhuongThucThanhToan
INSERT INTO PhuongThucThanhToan (TenPt, CreatedAt, UpdatedAt)
VALUES 
(N'Thanh toán qua thẻ', GETDATE(), GETDATE()), 
(N'Thanh toán qua ví điện tử', GETDATE(), GETDATE());
GO

-- Insert dữ liệu vào bảng KhachHang
INSERT INTO KhachHang (HoKh, TenKh, GioiTinh, Email, Sdt, DiaChi, TenNguoiDung, MatKhau, TrangThai, CreatedAt, UpdatedAt)
VALUES
(N'Nguyễn Văn', N'An', N'Nam', 'an.nguyen@example.com', '0912345678', N'123 Đường ABC, Quận XYZ, Thành phố Hà Nội', 'nguyenvanan', 'password123', 1, GETDATE(), GETDATE()),
(N'Trần Thị', N'Bình', N'Nữ', 'binh.tran@example.com', '0987654321', N'456 Đường DEF, Quận UVW, Thành phố Hồ Chí Minh', 'tranthibinh', 'pass456', 1, GETDATE(), GETDATE()),
(N'Lê Hoàng', N'Cường', N'Nam', 'cuong.le@example.com', '0933322111', N'789 Đường GHI, Quận RST, Thành phố Đà Nẵng', 'lehoangcuong', 'securepass789', 1, GETDATE(), GETDATE()),
(N'Phạm Thu', N'Dung', N'Nữ', 'dung.pham@example.com', '0977766555', N'101 Đường JKL, Quận MNO, Thành phố Cần Thơ', 'phamthudung', 'dung1234', 1, GETDATE(), GETDATE()),
(N'Hoàng Minh', N'Đức', N'Nam', 'duc.hoang@example.com', '0922211000', N'222 Đường PQR, Quận LMN, Thành phố Hải Phòng', 'hoangminhduc', 'duc5678', 1, GETDATE(), GETDATE()),
(N'Đỗ Ngọc', N'Hân', N'Nữ', 'han.do@example.com', '0944433222', N'333 Đường STU, Quận OPQ, Thành phố Hà Nội', 'dongochhan', 'han888', 1, GETDATE(), GETDATE()),
(N'Vũ Xuân', N'Kiên', N'Nam', 'kien.vu@example.com', '0966655444', N'444 Đường VWX, Quận YZA, Thành phố Hồ Chí Minh', 'vuxuankien', 'kien999', 1, GETDATE(), GETDATE()),
(N'Cao Thị', N'Loan', N'Nữ', 'loan.cao@example.com', '0955544333', N'555 Đường BCD, Quận EFG, Thành phố Đà Nẵng', 'caothiloan', 'loan222', 1, GETDATE(), GETDATE()),
(N'Trịnh Văn', N'Minh', N'Nam', 'minh.trinh@example.com', '0911100999', N'666 Đường HIJ, Quận KLM, Thành phố Cần Thơ', 'trinhvanminh', 'minh333', 1, GETDATE(), GETDATE()),
(N'Bùi Khánh', N'Ngân', N'Nữ', 'ngan.bui@example.com', '0988877666', N'777 Đường NOP, Quận QRS, Thành phố Hải Phòng', 'buikhanhngan', 'ngan444', 1, GETDATE(), GETDATE());
GO

-- Insert dữ liệu vào bảng DonHang
INSERT INTO DonHang (MaKh, NgayDatHang, TongTienDonHang, TrangThai, TrangThaiThanhToan, DiaChiNhanHang, PhuongThucBan, MaPt, GhiChu, SoDienThoai, CreatedAt, UpdatedAt)
VALUES
(1, GETDATE(), 1000000.00, 0, 0, N'123 Đường ABC, Quận XYZ, Thành phố Hà Nội', N'Mua trực tuyến', 1, N'Giao hàng giờ hành chính', '0912345678', GETDATE(), GETDATE()),
(2, GETDATE(), 2000000.00, 1, 1, N'456 Đường DEF, Quận UVW, Thành phố Hồ Chí Minh', N'Mua tại cửa hàng', 2, N'Khách hàng yêu cầu xuất hóa đơn', '0987654321', GETDATE(), GETDATE()),
(3, GETDATE(), 1500000.00, 0, 0, N'789 Đường GHI, Quận RST, Thành phố Đà Nẵng', N'Mua trực tuyến', 1, N'Gói quà tặng', '0933322111', GETDATE(), GETDATE()),
(4, GETDATE(), 2500000.00, 1, 1, N'101 Đường JKL, Quận MNO, Thành phố Cần Thơ', N'Mua tại cửa hàng', 2, N'Thanh toán bằng thẻ tín dụng', '0977766555', GETDATE(), GETDATE()),
(5, GETDATE(), 1200000.00, 0, 0, N'222 Đường PQR, Quận LMN, Thành phố Hải Phòng', N'Mua trực tuyến', 1, N'Giao hàng nhanh', '0922211000', GETDATE(), GETDATE()),
(6, GETDATE(), 1800000.00, 1, 0, N'333 Đường STU, Quận OPQ, Thành phố Hà Nội', N'Mua tại cửa hàng', 2, N'Khách hàng đến lấy', '0944433222', GETDATE(), GETDATE()),
(7, GETDATE(), 3000000.00, 1, 1, N'444 Đường VWX, Quận YZA, Thành phố Hồ Chí Minh', N'Mua trực tuyến', 1, N'Yêu cầu xuất hóa đơn VAT', '0966655444', GETDATE(), GETDATE()),
(8, GETDATE(), 900000.00, 0, 0, N'555 Đường BCD, Quận EFG, Thành phố Đà Nẵng', N'Mua tại cửa hàng', 2, N'Giao giờ cụ thể 17h-19h', '0955544333', GETDATE(), GETDATE()),
(9, GETDATE(), 2200000.00, 1, 1, N'666 Đường HIJ, Quận KLM, Thành phố Cần Thơ', N'Mua trực tuyến', 1, N'Thanh toán trực tuyến', '0911100999', GETDATE(), GETDATE()),
(10, GETDATE(), 1700000.00, 0, 0, N'777 Đường NOP, Quận QRS, Thành phố Hải Phòng', N'Mua tại cửa hàng', 2, N'Giao cuối tuần', '0988877666', GETDATE(), GETDATE());
GO

-- Insert dữ liệu vào bảng ChiTietDonHang
INSERT INTO ChiTietDonHang (MaDh, MaSp, SoLuong, TongTienTungSanPham)
VALUES
(1, 1, 2, 900000.00),  -- Đơn hàng 1: 2 sản phẩm 1
(1, 3, 1, 980000.00),  -- Đơn hàng 1: 1 sản phẩm 3
(2, 2, 1, 720000.00),  -- Đơn hàng 2: 1 sản phẩm 2
(2, 4, 3, 1950000.00), -- Đơn hàng 2: 3 sản phẩm 4
(3, 5, 2, 1000000.00),  -- Đơn hàng 3: 2 sản phẩm 5
(3, 7, 1, 880000.00),  -- Đơn hàng 3: 1 sản phẩm 7
(4, 10, 4, 1160000.00), -- Đơn hàng 4: 4 sản phẩm 10
(4, 1, 1, 450000.00),   -- Đơn hàng 4: 1 sản phẩm 1
(5, 6, 2, 780000.00),  -- Đơn hàng 5: 2 sản phẩm 6
(5, 8, 1, 550000.00),  -- Đơn hàng 5: 1 sản phẩm 8
(6, 9, 1, 1100000.00), -- Đơn hàng 6: 1 sản phẩm 9
(7, 3, 2, 1960000.00), -- Đơn hàng 7: 2 sản phẩm 3
(8, 2, 1, 720000.00),  -- Đơn hàng 8: 1 sản phẩm 2
(9, 5, 3, 1500000.00),  -- Đơn hàng 9: 3 sản phẩm 5
(10, 8, 2, 1100000.00); -- Đơn hàng 10: 2 sản phẩm 8
GO

-- Insert dữ liệu vào bảng ChiTietGioHang
INSERT INTO ChiTietGioHang (MaKh, MaSp, SoLuong, CreatedAt)
VALUES 
(1, 1, 3, GETDATE()), 
(2, 2, 5, GETDATE());
GO

-- Insert dữ liệu vào bảng SanPhamYeuThich
INSERT INTO SanPhamYeuThich (MaKh, MaSp)
VALUES 
(1, 1), 
(2, 2),
(3, 1),
(3, 3),
(4, 2),
(5, 5),
(5, 4),
(6, 1);
GO

-- Insert dữ liệu vào bảng BinhLuan
INSERT INTO BinhLuan (MaSp, MaKh, SoSao, NoiDung, Ngay, TrangThai, CreatedAt, UpdatedAt)
VALUES 
(1, 1, 5, N'Sản phẩm tuyệt vời!', GETDATE(), 1, GETDATE(), GETDATE()), 
(2, 2, 4, N'Sản phẩm tốt, giá hợp lý.', GETDATE(), 1, GETDATE(), GETDATE()),
(1, 3, 5, N'Tôi rất thích sản phẩm này!', GETDATE(), 1, GETDATE(), GETDATE()),
(3, 2, 3, N'Sản phẩm giao hàng hơi lâu.', GETDATE(), 1, GETDATE(), GETDATE()),
(4, 4, 4, N'Chất lượng sản phẩm ổn định.', GETDATE(), 1, GETDATE(), GETDATE()),
(5, 5, 5, N'Sản phẩm đáng mua!', GETDATE(), 1, GETDATE(), GETDATE()),
(2, 1, 4, N'Giá cả phải chăng.', GETDATE(), 1, GETDATE(), GETDATE()),
(1, 2, 5, N'Đóng gói cẩn thận.', GETDATE(), 1, GETDATE(), GETDATE());
GO

-- Insert dữ liệu vào bảng KhuyenMai
INSERT INTO KhuyenMai (MaSp, MucGiamGia, NgayBatDau, NgayKetThuc, CreatedAt, UpdatedAt)
VALUES 
(1, 10.00, '2025-05-01', '2025-06-01', GETDATE(), GETDATE()), 
(2, 15.00, '2025-05-01', '2025-06-01', GETDATE(), GETDATE()),
(3, 20.00, '2025-05-15', '2025-06-15', GETDATE(), GETDATE()),
(4, 5.00, '2025-06-01', '2025-07-01', GETDATE(), GETDATE()),
(5, 12.00, '2025-06-10', '2025-07-10', GETDATE(), GETDATE()),
(1, 8.00, '2025-07-01', '2025-07-31', GETDATE(), GETDATE()),
(2, 18.00, '2025-07-15', '2025-08-15', GETDATE(), GETDATE());

GO

-- Insert dữ liệu vào bảng BaiViet
INSERT INTO BaiViet (TieuDe, HinhAnh, NoiDung, NgayDang, CreatedAt, UpdatedAt)
VALUES 
(N'Top 5 món quà tặng được yêu thích nhất', 'top-5-qua-tang.jpg', N'Bài viết tổng hợp 5 món quà tặng được khách hàng ưa chuộng nhất trong thời gian qua. Bao gồm các sản phẩm hoa tươi, giỏ trái cây, quà tặng doanh nghiệp, v.v.', GETDATE(), GETDATE(), GETDATE()),
(N'Bí quyết chọn quà tặng cho người thân', 'bi-quyet-qua-tang.jpg', N'Bài viết chia sẻ những bí quyết và gợi ý để bạn có thể chọn được món quà tặng phù hợp và ý nghĩa nhất cho người thân của mình.', GETDATE(), GETDATE(), GETDATE()),
(N'Xu hướng quà tặng năm nay', 'xu-huong-qua-tang.jpg', N'Bài viết cập nhật những xu hướng quà tặng mới nhất trong năm, giúp bạn luôn bắt kịp thời đại và chọn được những món quà độc đáo.', GETDATE(), GETDATE(), GETDATE()),
(N'Quà tặng doanh nghiệp: Xây dựng mối quan hệ bền vững', 'qua-tang-doanh-nghiep.jpg', N'Bài viết về tầm quan trọng của quà tặng doanh nghiệp trong việc xây dựng và duy trì mối quan hệ tốt đẹp với đối tác và khách hàng.', GETDATE(), GETDATE(), GETDATE()),
(N'Hướng dẫn bảo quản quà tặng tươi lâu', 'bao-quan-qua-tang.jpg', N'Bài viết cung cấp những hướng dẫn chi tiết về cách bảo quản các loại quà tặng tươi như hoa, trái cây để giữ được vẻ đẹp và độ tươi ngon lâu nhất.', GETDATE(), GETDATE(), GETDATE()),
(N'Ý nghĩa của các loại hoa tặng trong những dịp đặc biệt', 'y-nghia-hoa-tang.jpg', N'Bài viết giải thích ý nghĩa của các loại hoa khác nhau và gợi ý lựa chọn hoa phù hợp cho từng dịp lễ, sự kiện đặc biệt.', GETDATE(), GETDATE(), GETDATE()),
(N'Gợi ý quà tặng cho người yêu', 'qua-tang-nguoi-yeu.jpg', N'Bài viết đưa ra những gợi ý quà tặng lãng mạn và ý nghĩa dành cho người yêu trong các dịp như Valentine, kỷ niệm ngày yêu, v.v.', GETDATE(), GETDATE(), GETDATE()),
(N'Quà tặng sức khỏe: Món quà ý nghĩa cho mọi người', 'qua-tang-suc-khoe.jpg', N'Bài viết giới thiệu những món quà tặng tốt cho sức khỏe, phù hợp để tặng cho mọi đối tượng và thể hiện sự quan tâm của bạn.', GETDATE(), GETDATE(), GETDATE());
GO

-- Insert dữ liệu vào bảng LienHe
INSERT INTO LienHe (HoTen, Email, Sdt, NoiDung, ThoiGianGui, TrangThai)
VALUES 
(N'Nguyễn Văn An', 'an.nguyen@example.com', '0912345678', N'Tôi muốn đặt hàng số lượng lớn.', GETDATE(), 0), 
(N'Trần Thị Bình', 'binh.tran@example.com', '0987654321', N'Tôi có câu hỏi về chính sách giao hàng.', GETDATE(), 1),
(N'Lê Hoàng Cường', 'cuong.le@example.com', '0933322111', N'Xin chào, tôi muốn biết thêm về sản phẩm quà tặng doanh nghiệp.', GETDATE(), 1),
(N'Phạm Thu Dung', 'dung.pham@example.com', '0977766555', N'Tôi cần hỗ trợ về đơn hàng đã đặt.', GETDATE(), 0),
(N'Hoàng Minh Đức', 'duc.hoang@example.com', '0922211000', N'Shop có chi nhánh ở Hà Nội không?', GETDATE(), 1),
(N'Đỗ Ngọc Hân', 'han.do@example.com', '0944433222', N'Tôi muốn phản hồi về chất lượng sản phẩm.', GETDATE(), 0),
(N'Vũ Xuân Kiên', 'kien.vu@example.com', '0966655444', N'Cửa hàng có dịch vụ gói quà theo yêu cầu không?', GETDATE(), 1),
(N'Cao Thị Loan', 'loan.cao@example.com', '0955544333', N'Tôi muốn đặt quà tặng cho sự kiện công ty.', GETDATE(), 1),
(N'Trịnh Văn Minh', 'minh.trinh@example.com', '0911100999', N'Làm thế nào để hủy đơn hàng?', GETDATE(), 0),
(N'Bùi Khánh Ngân', 'ngan.bui@example.com', '0988877666', N'Shop có chương trình khuyến mãi nào cho khách hàng mới không?', GETDATE(), 1);
GO

-- Insert dữ liệu vào bảng DichVuCongTy
INSERT INTO DichVuCongTy (TieuDe, MoTa, HinhAnh, TrangThai, CreatedAt, UpdatedAt)
VALUES 
(N'Giao hàng nhanh chóng', N'Chúng tôi cam kết giao hàng đến tay khách hàng trong thời gian ngắn nhất.', 'giao-hang-nhanh.jpg', 1, GETDATE(), GETDATE()), 
(N'Tư vấn quà tặng chuyên nghiệp', N'Đội ngũ chuyên viên giàu kinh nghiệm sẽ giúp bạn chọn được món quà ưng ý.', 'tu-van-qua-tang.jpg', 1, GETDATE(), GETDATE()),
(N'Gói quà theo yêu cầu', N'Chúng tôi cung cấp dịch vụ gói quà độc đáo, thể hiện sự chu đáo của bạn.', 'goi-qua-theo-yeu-cau.jpg', 1, GETDATE(), GETDATE()),
(N'Thiết kế quà tặng doanh nghiệp', N'Giải pháp quà tặng sáng tạo, giúp doanh nghiệp xây dựng mối quan hệ bền vững.', 'qua-tang-doanh-nghiep.jpg', 1, GETDATE(), GETDATE()),
(N'Dịch vụ điện hoa', N'Gửi tặng những bó hoa tươi thắm kèm quà tặng, trao gửi yêu thương.', 'dich-vu-dien-hoa.jpg', 1, GETDATE(), GETDATE()),
(N'Chương trình khách hàng thân thiết', N'Ưu đãi đặc biệt dành cho khách hàng thường xuyên của chúng tôi.', 'khach-hang-than-thiet.jpg', 1, GETDATE(), GETDATE()),
(N'Hỗ trợ 24/7', N'Đội ngũ hỗ trợ luôn sẵn sàng giải đáp mọi thắc mắc của bạn.', 'ho-tro-24-7.jpg', 1, GETDATE(), GETDATE()),
(N'Quà tặng sự kiện', N'Giải pháp quà tặng cho mọi sự kiện, từ sinh nhật, kỷ niệm đến khai trương.', 'qua-tang-su-kien.jpg', 1, GETDATE(), GETDATE()),
(N'Dịch vụ in ấn logo', N'In logo và thông điệp lên quà tặng, tăng cường nhận diện thương hiệu.', 'in-an-logo.jpg', 1, GETDATE(), GETDATE()),
(N'Giao hàng quốc tế', N'Chúng tôi giao quà tặng đến mọi nơi trên thế giới, kết nối yêu thương không khoảng cách.', 'giao-hang-quoc-te.jpg', 1, GETDATE(), GETDATE());



-- Lấy toàn bộ dữ liệu từ bảng Header
SELECT * FROM Header;
GO

-- Lấy toàn bộ dữ liệu từ bảng Footer
SELECT * FROM Footer;
GO

-- Lấy toàn bộ dữ liệu từ bảng DanhMuc
SELECT * FROM DanhMuc;
GO

-- Lấy toàn bộ dữ liệu từ bảng NhaCungCap
SELECT * FROM NhaCungCap;
GO

-- Lấy toàn bộ dữ liệu từ bảng SanPham
SELECT * FROM SanPham;
GO

-- Lấy toàn bộ dữ liệu từ bảng PhuongThucThanhToan
SELECT * FROM PhuongThucThanhToan;
GO

-- Lấy toàn bộ dữ liệu từ bảng KhachHang
SELECT * FROM KhachHang;
GO

-- Lấy toàn bộ dữ liệu từ bảng DonHang
SELECT * FROM DonHang;
GO

-- Lấy toàn bộ dữ liệu từ bảng ChiTietDonHang
SELECT * FROM ChiTietDonHang;
GO

-- Lấy toàn bộ dữ liệu từ bảng ChiTietGioHang
SELECT * FROM ChiTietGioHang;
GO

-- Lấy toàn bộ dữ liệu từ bảng SanPhamYeuThich
SELECT * FROM SanPhamYeuThich;
GO

-- Lấy toàn bộ dữ liệu từ bảng BinhLuan
SELECT * FROM BinhLuan;
GO

-- Lấy toàn bộ dữ liệu từ bảng KhuyenMai
SELECT * FROM KhuyenMai;
GO

-- Lấy toàn bộ dữ liệu từ bảng BaiViet
SELECT * FROM BaiViet;
GO

-- Lấy toàn bộ dữ liệu từ bảng LienHe
SELECT * FROM LienHe;
GO

-- Lấy toàn bộ dữ liệu từ bảng DichVuCongTy
SELECT * FROM DichVuCongTy;
GO