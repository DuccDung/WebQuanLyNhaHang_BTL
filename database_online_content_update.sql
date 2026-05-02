USE [QLNhaHang_BTL];
GO

/* =========================================================
   Cloudy Cafe - Online website data update
   Muc dich:
   1. Dua trang CuaHang tu HTML tinh vao database.
   2. Dua trang ChuyenNha tu HTML tinh vao database.
   3. Chuan hoa thong tin giao hang online dang nam trong DonHang.GhiChu.
   ========================================================= */

IF OBJECT_ID(N'[dbo].[CuaHang]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CuaHang](
        [CuaHang_ID] [int] IDENTITY(1,1) NOT NULL,
        [TenCuaHang] [nvarchar](150) NOT NULL,
        [DiaChi] [nvarchar](300) NOT NULL,
        [TinhThanh] [nvarchar](80) NULL,
        [QuanHuyen] [nvarchar](80) NULL,
        [PhuongXa] [nvarchar](80) NULL,
        [SoDienThoai] [varchar](20) NULL,
        [GioMoCua] [time](0) NULL,
        [GioDongCua] [time](0) NULL,
        [PathPhoto] [nvarchar](500) NULL,
        [GoogleMapUrl] [nvarchar](700) NULL,
        [Latitude] [decimal](10, 7) NULL,
        [Longitude] [decimal](10, 7) NULL,
        [SapXep] [int] NOT NULL CONSTRAINT [DF_CuaHang_SapXep] DEFAULT ((0)),
        [HienThi] [bit] NOT NULL CONSTRAINT [DF_CuaHang_HienThi] DEFAULT ((1)),
        [Remove] [bit] NOT NULL CONSTRAINT [DF_CuaHang_Remove] DEFAULT ((0)),
        [CreatedAt] [datetime] NOT NULL CONSTRAINT [DF_CuaHang_CreatedAt] DEFAULT (GETDATE()),
        [UpdatedAt] [datetime] NULL,
        CONSTRAINT [PK_CuaHang] PRIMARY KEY CLUSTERED ([CuaHang_ID] ASC)
    );
END
GO

IF OBJECT_ID(N'[dbo].[BaiVietChuyenNha]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[BaiVietChuyenNha](
        [BaiViet_ID] [int] IDENTITY(1,1) NOT NULL,
        [TieuDe] [nvarchar](200) NOT NULL,
        [Slug] [varchar](220) NULL,
        [TomTat] [nvarchar](600) NULL,
        [NoiDung] [nvarchar](max) NULL,
        [PathPhoto] [nvarchar](500) NULL,
        [AltText] [nvarchar](200) NULL,
        [TacGia] [nvarchar](100) NULL,
        [NgayDang] [datetime] NOT NULL CONSTRAINT [DF_BaiVietChuyenNha_NgayDang] DEFAULT (GETDATE()),
        [NoiBat] [bit] NOT NULL CONSTRAINT [DF_BaiVietChuyenNha_NoiBat] DEFAULT ((0)),
        [SapXep] [int] NOT NULL CONSTRAINT [DF_BaiVietChuyenNha_SapXep] DEFAULT ((0)),
        [HienThi] [bit] NOT NULL CONSTRAINT [DF_BaiVietChuyenNha_HienThi] DEFAULT ((1)),
        [Remove] [bit] NOT NULL CONSTRAINT [DF_BaiVietChuyenNha_Remove] DEFAULT ((0)),
        [CreatedAt] [datetime] NOT NULL CONSTRAINT [DF_BaiVietChuyenNha_CreatedAt] DEFAULT (GETDATE()),
        [UpdatedAt] [datetime] NULL,
        CONSTRAINT [PK_BaiVietChuyenNha] PRIMARY KEY CLUSTERED ([BaiViet_ID] ASC)
    );
END
GO

IF OBJECT_ID(N'[dbo].[OnlineOrderInfo]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[OnlineOrderInfo](
        [OnlineOrderInfo_ID] [int] IDENTITY(1,1) NOT NULL,
        [DH_ID] [int] NOT NULL,
        [CuaHang_ID] [int] NULL,
        [TrangThaiGiaoHang] [varchar](30) NOT NULL CONSTRAINT [DF_OnlineOrderInfo_TrangThai] DEFAULT ('cart'),
        [NguoiNhan] [nvarchar](100) NULL,
        [SoDienThoai] [varchar](20) NULL,
        [TinhThanh] [nvarchar](80) NULL,
        [QuanHuyen] [nvarchar](80) NULL,
        [PhuongXa] [nvarchar](80) NULL,
        [DiaChi] [nvarchar](300) NULL,
        [GhiChuGiaoHang] [nvarchar](300) NULL,
        [PhiGiaoHang] [money] NOT NULL CONSTRAINT [DF_OnlineOrderInfo_PhiGiaoHang] DEFAULT ((0)),
        [PhuongThucThanhToan] [varchar](30) NULL,
        [TrangThaiThanhToan] [varchar](30) NULL,
        [NgayDat] [datetime] NULL,
        [NgayCapNhat] [datetime] NULL,
        [Remove] [bit] NOT NULL CONSTRAINT [DF_OnlineOrderInfo_Remove] DEFAULT ((0)),
        CONSTRAINT [PK_OnlineOrderInfo] PRIMARY KEY CLUSTERED ([OnlineOrderInfo_ID] ASC),
        CONSTRAINT [UQ_OnlineOrderInfo_DH_ID] UNIQUE ([DH_ID]),
        CONSTRAINT [FK_OnlineOrderInfo_DonHang] FOREIGN KEY ([DH_ID]) REFERENCES [dbo].[DonHang]([DH_ID]),
        CONSTRAINT [FK_OnlineOrderInfo_CuaHang] FOREIGN KEY ([CuaHang_ID]) REFERENCES [dbo].[CuaHang]([CuaHang_ID]),
        CONSTRAINT [CK_OnlineOrderInfo_TrangThaiGiaoHang] CHECK ([TrangThaiGiaoHang] IN ('cart','pending','preparing','shipping','delivered','cancelled'))
    );
END
GO

IF OBJECT_ID(N'[dbo].[OnlineOrderStatusHistory]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[OnlineOrderStatusHistory](
        [History_ID] [int] IDENTITY(1,1) NOT NULL,
        [DH_ID] [int] NOT NULL,
        [TrangThaiCu] [varchar](30) NULL,
        [TrangThaiMoi] [varchar](30) NOT NULL,
        [NV_ID] [int] NULL,
        [GhiChu] [nvarchar](300) NULL,
        [CreatedAt] [datetime] NOT NULL CONSTRAINT [DF_OnlineOrderStatusHistory_CreatedAt] DEFAULT (GETDATE()),
        CONSTRAINT [PK_OnlineOrderStatusHistory] PRIMARY KEY CLUSTERED ([History_ID] ASC),
        CONSTRAINT [FK_OnlineOrderStatusHistory_DonHang] FOREIGN KEY ([DH_ID]) REFERENCES [dbo].[DonHang]([DH_ID]),
        CONSTRAINT [FK_OnlineOrderStatusHistory_NhanVien] FOREIGN KEY ([NV_ID]) REFERENCES [dbo].[NhanVien]([NV_ID])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[CuaHang])
BEGIN
    INSERT INTO [dbo].[CuaHang]
        ([TenCuaHang], [DiaChi], [TinhThanh], [QuanHuyen], [SoDienThoai], [GioMoCua], [GioDongCua], [PathPhoto], [GoogleMapUrl], [SapXep])
    VALUES
        (N'Cloudy Café Nguyễn Huệ', N'123 Nguyễn Huệ, Q.1, TP.HCM', N'TP.HCM', N'Q.1', '02838234567', '07:00', '22:00', N'https://images.unsplash.com/photo-1554118811-1e0d58224f24?w=400', N'https://maps.google.com', 1),
        (N'Cloudy Café Lê Lợi', N'456 Lê Lợi, Q.1, TP.HCM', N'TP.HCM', N'Q.1', '02838256789', '06:30', '23:00', N'https://images.unsplash.com/photo-1559925393-8be0ec4767c8?w=400', N'https://maps.google.com', 2),
        (N'Cloudy Café Vincom Center', N'72 Lê Thánh Tôn, Q.1, TP.HCM', N'TP.HCM', N'Q.1', '02839369999', '08:00', '22:00', N'https://images.unsplash.com/photo-1511920170033-f8396924c348?w=400', N'https://maps.google.com', 3),
        (N'Cloudy Café Landmark 81', N'720A Điện Biên Phủ, Bình Thạnh, TP.HCM', N'TP.HCM', N'Bình Thạnh', '02836368888', '07:00', '23:00', N'https://images.unsplash.com/photo-1501339847302-ac426a4a7cbb?w=400', N'https://maps.google.com', 4),
        (N'Cloudy Café Phú Mỹ Hưng', N'Đường Nguyễn Văn Linh, Q.7, TP.HCM', N'TP.HCM', N'Q.7', '02854132468', '07:00', '22:00', N'https://images.unsplash.com/photo-1521017432531-fbd92d768814?w=400', N'https://maps.google.com', 5),
        (N'Cloudy Café Thảo Điền', N'159 Xuân Thủy, Thảo Điền, Q.2, TP.HCM', N'TP.HCM', N'Q.2', '02837445566', '06:30', '22:30', N'https://images.unsplash.com/photo-1504753793650-d4a2b783c15e?w=400', N'https://maps.google.com', 6);
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[BaiVietChuyenNha])
BEGIN
    INSERT INTO [dbo].[BaiVietChuyenNha]
        ([TieuDe], [Slug], [TomTat], [PathPhoto], [AltText], [TacGia], [NoiBat], [SapXep])
    VALUES
        (N'Câu chuyện về ly cà phê sáng', 'cau-chuyen-ve-ly-ca-phe-sang', N'Mỗi buổi sáng đều là một khởi đầu mới, và không gì tuyệt vời hơn việc bắt đầu ngày mới với một ly cà phê thơm ngon từ Cloudy Café.', N'https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=600', N'Câu chuyện về ly cà phê sáng', N'Cloudy Café', 1, 1),
        (N'Hành trình tìm kiếm hạt cà phê hoàn hảo', 'hanh-trinh-tim-kiem-hat-ca-phe-hoan-hao', N'Chúng tôi đi khắp các vùng cao nguyên để tìm kiếm những hạt cà phê thượng hạng nhất, mang đến trải nghiệm trọn vị trong từng ly.', N'https://images.unsplash.com/photo-1447933601403-0c6688de566e?w=600', N'Hành trình tìm kiếm hạt cà phê hoàn hảo', N'Cloudy Café', 0, 2),
        (N'Bí mật của ly trà xanh Tây Bắc', 'bi-mat-cua-ly-tra-xanh-tay-bac', N'Trà xanh Tây Bắc không chỉ là thức uống, mà còn là tinh hoa của đất trời, được chăm sóc kỹ lưỡng bởi những người làm trà tận tâm.', N'https://images.unsplash.com/photo-1564890369478-c89ca6d9cde9?w=600', N'Bí mật của ly trà xanh Tây Bắc', N'Cloudy Café', 0, 3),
        (N'Không gian Cloudy Café - Nơi gặp gỡ và sáng tạo', 'khong-gian-cloudy-cafe-noi-gap-go-va-sang-tao', N'Với thiết kế ấm cúng và không gian thoáng đãng, Cloudy Café là điểm đến lý tưởng cho những cuộc gặp gỡ bạn bè hay làm việc sáng tạo.', N'https://images.unsplash.com/photo-1511920170033-f8396924c348?w=600', N'Không gian Cloudy Café', N'Cloudy Café', 0, 4);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CuaHang_HienThi_Remove_SapXep' AND object_id = OBJECT_ID(N'[dbo].[CuaHang]'))
BEGIN
    CREATE INDEX [IX_CuaHang_HienThi_Remove_SapXep]
    ON [dbo].[CuaHang]([HienThi], [Remove], [SapXep])
    WHERE [Remove] = 0;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BaiVietChuyenNha_HienThi_Remove_SapXep' AND object_id = OBJECT_ID(N'[dbo].[BaiVietChuyenNha]'))
BEGIN
    CREATE INDEX [IX_BaiVietChuyenNha_HienThi_Remove_SapXep]
    ON [dbo].[BaiVietChuyenNha]([HienThi], [Remove], [SapXep])
    WHERE [Remove] = 0;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OnlineOrderInfo_TrangThaiGiaoHang' AND object_id = OBJECT_ID(N'[dbo].[OnlineOrderInfo]'))
BEGIN
    CREATE INDEX [IX_OnlineOrderInfo_TrangThaiGiaoHang]
    ON [dbo].[OnlineOrderInfo]([TrangThaiGiaoHang], [Remove]);
END
GO
