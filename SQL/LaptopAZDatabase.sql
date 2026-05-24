-- ========================================================================
-- KỊCH BẢN KHỞI TẠO DATABASE HỆ THỐNG QUẢN LÝ CỬA HÀNG LAPTOP (LAPTOPAZ)
-- Chuẩn hóa: 3NF & Tối ưu hóa Nghiệp vụ Bảo hành/Serial thực tế
-- ========================================================================

-- ========================================================================
-- 1. NHÓM PHÂN QUYỀN & NGƯỜI DÙNG (Authentication & Authorization)
-- ========================================================================

CREATE TABLE Roles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) UNIQUE NOT NULL
);

CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username VARCHAR(50) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Phone VARCHAR(15),
    Email VARCHAR(100),
    RoleId INT NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

-- ========================================================================
-- 2. NHÓM SẢN PHẨM & DANH MỤC (Product Catalog)
-- ========================================================================

CREATE TABLE Categories (
    CategoryId INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) UNIQUE NOT NULL
);

CREATE TABLE Brands (
    BrandId INT PRIMARY KEY IDENTITY(1,1),
    BrandName NVARCHAR(100) UNIQUE NOT NULL
);

-- Bảng Products đóng vai trò là "Cấu hình Model gốc" (SKU)
CREATE TABLE Products (
    ProductId INT PRIMARY KEY IDENTITY(1,1),
    ProductCode VARCHAR(50) UNIQUE NOT NULL,
    ProductName NVARCHAR(255) NOT NULL,
    CategoryId INT NOT NULL,
    BrandId INT NOT NULL,
    
    -- Chi tiết cấu hình phần cứng độc lập
    CPU NVARCHAR(255) NOT NULL,
    RAM NVARCHAR(255) NOT NULL,
    GPU NVARCHAR(255),
    Storage NVARCHAR(255) NOT NULL,
    ScreenSize NVARCHAR(255),
    
    ImportPrice DECIMAL(18,2) NOT NULL, -- Giá nhập tham chiếu/mặc định
    SalePrice DECIMAL(18,2) NOT NULL,   -- Giá bán hiện tại công bố
    QuantityInStock INT DEFAULT 0,      -- *Thuộc tính suy dẫn* (Giữ lại để tăng tốc hiệu năng hệ thống)
    
    ImageUrl VARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    
    FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId),
    FOREIGN KEY (BrandId) REFERENCES Brands(BrandId)
);

-- ========================================================================
-- 3. NHÓM ĐỐI TÁC (Suppliers & Customers)
-- ========================================================================

CREATE TABLE Suppliers (
    SupplierId INT PRIMARY KEY IDENTITY(1,1),
    SupplierName NVARCHAR(255) NOT NULL,
    Phone VARCHAR(15) NOT NULL,
    Email VARCHAR(100),
    Address NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Tách riêng bảng Customers để loại bỏ phụ thuộc bắc cầu trong đơn hàng (Đạt chuẩn 3NF)
CREATE TABLE Customers (
    CustomerId INT PRIMARY KEY IDENTITY(1,1),
    CustomerName NVARCHAR(100) NOT NULL,
    Phone VARCHAR(15) UNIQUE NOT NULL,
    Email VARCHAR(100),
    Address NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- ========================================================================
-- 4. NHÓM QUẢN LÝ NHẬP KHO (Procurement / Inbound)
-- ========================================================================

CREATE TABLE ImportReceipts (
    ImportReceiptId INT PRIMARY KEY IDENTITY(1,1),
    SupplierId INT NOT NULL,
    CreatedBy INT NOT NULL,
    ImportDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) DEFAULT 0, -- *Thuộc tính suy dẫn* (Tổng tiền của phiếu nhập)
    
    FOREIGN KEY (SupplierId) REFERENCES Suppliers(SupplierId),
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);

CREATE TABLE ImportReceiptDetails (
    ImportDetailId INT PRIMARY KEY IDENTITY(1,1),
    ImportReceiptId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    ImportPrice DECIMAL(18,2) NOT NULL, -- Giá nhập thực tế tại thời điểm nhập của lô này
    
    FOREIGN KEY (ImportReceiptId) REFERENCES ImportReceipts(ImportReceiptId),
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

-- ========================================================================
-- 5. NHÓM QUẢN LÝ BÁN HÀNG (Sales / Outbound)
-- ========================================================================

CREATE TABLE Orders (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    OrderCode VARCHAR(50) UNIQUE NOT NULL,
    CustomerId INT NOT NULL, -- Liên kết sang bảng khách hàng (Đúng chuẩn 3NF)
    CreatedBy INT NOT NULL,  -- Nhân viên trực tiếp lên đơn
    OrderDate DATETIME DEFAULT GETDATE(),
    
    TotalAmount DECIMAL(18,2) DEFAULT 0,    -- *Thuộc tính suy dẫn* (Tổng tiền trước chiết khấu)
    DiscountAmount DECIMAL(18,2) DEFAULT 0, -- Tiền giảm giá / Voucher
    FinalAmount DECIMAL(18,2) DEFAULT 0,    -- *Thuộc tính suy dẫn* (Tiền thực tế phải trả)
    Status VARCHAR(30) NOT NULL,            -- Ví dụ: 'Pending', 'Paid', 'Cancelled'
    
    FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);

CREATE TABLE OrderDetails (
    OrderDetailId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL, -- Giá bán thực tế đã chốt với khách tại thời điểm mua
    
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

-- ========================================================================
-- 6. QUẢN LÝ THIẾT BỊ VẬT LÝ BẰNG SỐ SERIAL (Core Business Logic)
-- Bảng then chốt để quản lý chính xác từng chiếc Laptop bán ra (Phục vụ Bảo hành)
-- ========================================================================

CREATE TABLE ProductItems (
    SerialNumber VARCHAR(50) PRIMARY KEY, -- Số Serial duy nhất định danh từng chiếc laptop
    ProductId INT NOT NULL,
    ImportDetailId INT NOT NULL,          -- Xác định máy nằm trong lô nhập nào, giá vốn bao nhiêu
    OrderDetailId INT NULL,               -- NULL nếu chưa bán, điền ID nếu máy đã có chủ đơn nhận
    Status VARCHAR(30) DEFAULT 'InStock', -- 'InStock', 'Sold', 'Warranty', 'Defective'
    
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    FOREIGN KEY (ImportDetailId) REFERENCES ImportReceiptDetails(ImportDetailId),
    FOREIGN KEY (OrderDetailId) REFERENCES OrderDetails(OrderDetailId)
);

-- ========================================================================
-- 7. LỊCH SỬ KHO & QUẢN LÝ ĐỔI TRẢ (Inventory Logs & Returns)
-- ========================================================================

CREATE TABLE InventoryLogs (
    InventoryLogId INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    ChangeType VARCHAR(30) NOT NULL,     -- 'IMPORT', 'EXPORT', 'RETURN_CUSTOMER', 'RETURN_SUPPLIER'
    QuantityChanged INT NOT NULL,        -- Số lượng biến động (+ hoặc -)
    ReferenceId INT NOT NULL,            -- Lưu Id của Phiếu nhập hoặc Hóa đơn sinh ra biến động này
    Note NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE(),
    
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

CREATE TABLE Returns (
    ReturnId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    CreatedBy INT NOT NULL,
    ReturnDate DATETIME DEFAULT GETDATE(),
    Reason NVARCHAR(255) NOT NULL,
    
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);

CREATE TABLE ReturnDetails (
    ReturnDetailId INT PRIMARY KEY IDENTITY(1,1),
    ReturnId INT NOT NULL,
    SerialNumber VARCHAR(50) NOT NULL, -- Chỉ rõ chính xác con máy mang số Serial nào bị trả lại
    Quantity INT DEFAULT 1,            -- Hàng quản lý theo Serial thì mặc định mỗi dòng là 1 máy
    
    FOREIGN KEY (ReturnId) REFERENCES Returns(ReturnId),
    FOREIGN KEY (SerialNumber) REFERENCES ProductItems(SerialNumber)
);
GO

-- ========================================================================
-- 8. TẠO TẬP INDEX ĐỂ TỐI ƯU HÓA TRUY VẤN (Performance Tuning)
-- ========================================================================

-- Tăng tốc độ tìm kiếm hóa đơn, sản phẩm và tài khoản người dùng
CREATE NONCLUSTERED INDEX IX_Products_ProductCode ON Products(ProductCode);
CREATE NONCLUSTERED INDEX IX_Orders_OrderCode ON Orders(OrderCode);
CREATE NONCLUSTERED INDEX IX_Users_Username ON Users(Username);

-- Tăng tốc độ tra cứu lịch sử mua hàng và tìm kiếm thông tin theo số điện thoại khách hàng
CREATE NONCLUSTERED INDEX IX_Customers_Phone ON Customers(Phone);
CREATE NONCLUSTERED INDEX IX_ProductItems_Status ON ProductItems(Status);
GO

USE LaptopAZDB;
GO

-- ========================================================================
-- 1. CHÈN DỮ LIỆU BẢNG ROLES (VAI TRÒ)
-- ========================================================================
INSERT INTO Roles (RoleName) VALUES 
('Admin'),
('WarehouseStaff'),
('SalesStaff');

-- ========================================================================
-- 2. CHÈN DỮ LIỆU BẢNG USERS (NHÂN VIÊN)
-- (Mã Role tự tăng: 1 = Admin, 2 = Kho, 3 = Bán hàng)
-- ========================================================================
INSERT INTO Users (Username, PasswordHash, FullName, Phone, Email, RoleId) VALUES
('tuan.leadmin', 'pbkdf2_sha256$260000$adminhashpwd123', N'Lê Minh Tuấn', '0912345678', 'tuan.le@laptopaz.vn', 1),
('mai.nguyenkho', 'pbkdf2_sha256$260000$warehousehash456', N'Nguyễn Thị Phương Mai', '0987654321', 'mai.ntp@laptopaz.vn', 2),
('nam.tranbanhang', 'pbkdf2_sha256$260000$saleshash789', N'Trần Hải Nam', '0905123456', 'nam.th@laptopaz.vn', 3);

-- ========================================================================
-- 3. CHÈN DỮ LIỆU BẢNG CATEGORIES (DANH MỤC)
-- ========================================================================
INSERT INTO Categories (CategoryName) VALUES
(N'Laptop Gaming'),
(N'Laptop Văn Phòng'),
(N'Workstation Đồ Họa');

-- ========================================================================
-- 4. CHÈN DỮ LIỆU BẢNG BRANDS (THƯƠNG HIỆU)
-- ========================================================================
INSERT INTO Brands (BrandName) VALUES
('ASUS'),
('Dell'),
('Apple'),
('Lenovo');

-- ========================================================================
-- 5. CHÈN DỮ LIỆU BẢNG PRODUCTS (CẤU HÌNH MÁY GỐC - SKU)
-- (CategoryId: 1=Gaming, 2=Office, 3=Workstation | BrandId: 1=ASUS, 2=Dell, 3=Apple, 4=Lenovo)
-- ========================================================================
INSERT INTO Products (ProductCode, ProductName, CategoryId, BrandId, CPU, RAM, GPU, Storage, ScreenSize, ImportPrice, SalePrice, QuantityInStock) VALUES
('ASUS-ROG-G16', N'ASUS ROG Strix G16 G614JV', 1, 1, 'Intel Core i7-13650HX', '16GB DDR5', 'NVIDIA RTX 4060 8GB', '512GB NVMe PCIe', '16.0 inch FHD+ 165Hz', 28000000, 32490000, 3),
('DELL-INSP-14', N'Dell Inspiron 14 5430', 2, 2, 'Intel Core i5-1340P', '16GB LPDDR5', 'Intel Iris Xe Graphics', '512GB SSD', '14.0 inch FHD+', 13500000, 15990000, 2),
('MAC-AIR-M3', N'Apple MacBook Air 13 inch M3', 2, 3, 'Apple M3 8-Core', '16GB Unified', '10-Core GPU', '512GB SSD', '13.6 inch Liquid Retina', 29000000, 32990000, 2),
('THINKPAD-P16', N'Lenovo ThinkPad P16 Gen 2', 3, 4, 'Intel Core i9-13980HX', '32GB DDR5', 'NVIDIA RTX A1000 6GB', '1TB NVMe SSD', '16.0 inch WUXGA IPS', 52000000, 59500000, 1);

-- ========================================================================
-- 6. CHÈN DỮ LIỆU BẢNG SUPPLIERS (NHÀ CUNG CẤP)
-- ========================================================================
INSERT INTO Suppliers (SupplierName, Phone, Email, Address) VALUES
(N'Công ty Cổ phần Máy tính Vĩnh Xuân', '02439434112', 'info@spc.com.vn', N'39 Trần Quốc Toản, Hoàn Kiếm, Hà Nội'),
(N'Nhà phân phối Synnex FPT', '02473007300', 'contact@synnexfpt.com.vn', N'Tòa nhà FPT, Khu công nghiệp Cầu Giấy, Hà Nội');

-- ========================================================================
-- 7. CHÈN DỮ LIỆU BẢNG CUSTOMERS (KHÁCH HÀNG)
-- ========================================================================
INSERT INTO Customers (CustomerName, Phone, Email, Address) VALUES
(N'Hoàng Minh Triết', '0945112233', 'triethm@gmail.com', N'12 Chùa Bộc, Đống Đa, Hà Nội'),
(N'Phan Thanh Thảo', '0966889900', 'thaopt99@yahoo.com', N'Toà nhà Landmark 81, Bình Thạnh, TP.HCM'),
(N'Nguyễn Hoàng Long', '0911556677', 'longnh.tech@gmail.com', N'Phố Trần Đại Nghĩa, Hai Bà Trưng, Hà Nội');

-- ========================================================================
-- 8. NHÓM ĐƠN NHẬP KHO (IMPORT RECEIPTS)
-- Nhân viên Phương Mai (UserId = 2) nhập hàng từ Vĩnh Xuân (SupplierId = 1)
-- ========================================================================
INSERT INTO ImportReceipts (SupplierId, CreatedBy, ImportDate, TotalAmount) VALUES
(1, 2, '2026-05-10 09:30:00', 169000000); 
-- Tổng tiền: (3 máy ASUS * 28tr) + (2 máy Dell * 13.5tr) + (2 máy Mac * 29tr) = 169,000,000

INSERT INTO ImportReceiptDetails (ImportReceiptId, ProductId, Quantity, ImportPrice) VALUES
(1, 1, 3, 28000000), -- ImportDetailId = 1
(1, 2, 2, 13500000), -- ImportDetailId = 2
(1, 3, 2, 29000000); -- ImportDetailId = 3

-- ========================================================================
-- 9. NHÓM ĐƠN BÁN HÀNG (ORDERS)
-- Nhân viên Hải Nam (UserId = 3) lên 2 đơn hàng cho khách Triết và khách Thảo
-- ========================================================================
INSERT INTO Orders (OrderCode, CustomerId, CreatedBy, OrderDate, TotalAmount, DiscountAmount, FinalAmount, Status) VALUES
('HD-20260515-001', 1, 3, '2026-05-15 14:20:00', 32490000, 500000, 31990000, 'Paid'),
('HD-20260516-002', 2, 3, '2026-05-16 10:15:00', 15990000, 0, 15990000, 'Paid');

INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice) VALUES
(1, 1, 1, 32490000), -- OrderDetailId = 1 (Đơn 1 mua 1 máy ASUS ROG)
(2, 2, 1, 15990000); -- OrderDetailId = 2 (Đơn 2 mua 1 máy Dell Inspiron)

-- ========================================================================
-- 10. QUẢN LÝ ĐỊNH DANH VẬT LÝ TỪNG CHIẾC LAPTOP (PRODUCT ITEMS)
-- Khớp nối số Serial của máy với đúng lô nhập và đơn bán thực tế
-- ========================================================================
INSERT INTO ProductItems (SerialNumber, ProductId, ImportDetailId, OrderDetailId, Status) VALUES
-- Lô máy ASUS ROG (ImportDetailId = 1): Có 3 máy về kho
('LR0Y1X8A', 1, 1, 1, 'Sold'),          -- Chiếc này đã bán đi theo Đơn 1 (OrderDetailId = 1)
('LR0Y2X9B', 1, 1, NULL, 'InStock'),     -- Chiếc này đang nằm trên kệ kho
('LR0Y3X0C', 1, 1, NULL, 'InStock'),     -- Chiếc này đang nằm trên kệ kho

-- Lô máy Dell Inspiron (ImportDetailId = 2): Có 2 máy về kho
('5CZ7421XYZ', 2, 2, 2, 'Sold'),        -- Chiếc này đã bán đi theo Đơn 2 (OrderDetailId = 2)
('5CZ7422ABC', 2, 2, NULL, 'InStock'),   -- Chiếc này còn trong kho

-- Lô máy MacBook Air M3 (ImportDetailId = 3): Có 2 máy về kho
('C02FX123Q6A1', 3, 3, NULL, 'InStock'), -- Chưa bán
('C02FX456Q6A2', 3, 3, NULL, 'InStock'); -- Chưa bán

-- ========================================================================
-- 11. THEO DÕI BIẾN ĐỘNG KHO (INVENTORY LOGS)
-- ========================================================================
INSERT INTO InventoryLogs (ProductId, ChangeType, QuantityChanged, ReferenceId, Note) VALUES
(1, 'IMPORT', 3, 1, N'Nhập kho lô hàng ASUS ROG từ Vĩnh Xuân'),
(2, 'IMPORT', 2, 1, N'Nhập kho lô hàng Dell Inspiron từ Vĩnh Xuân'),
(3, 'IMPORT', 2, 1, N'Nhập kho lô hàng MacBook Air từ Vĩnh Xuân'),
(1, 'EXPORT', -1, 1, N'Xuất kho máy Serial LR0Y1X8A giao đơn HD-20260515-001'),
(2, 'EXPORT', -1, 2, N'Xuất kho máy Serial 5CZ7421XYZ giao đơn HD-20260516-002');

-- ========================================================================
-- 12. XỬ LÝ ĐỔI TRẢ (RETURNS)
-- Khách Phan Thanh Thảo (Đơn 2) đem trả lại chiếc Dell có Serial '5CZ7421XYZ'
-- ========================================================================
INSERT INTO Returns (OrderId, CreatedBy, ReturnDate, Reason) VALUES
(2, 3, '2026-05-18 16:00:00', N'Màn hình xuất hiện sọc chỉ xanh sau 2 ngày sử dụng');

INSERT INTO ReturnDetails (ReturnId, SerialNumber, Quantity) VALUES
(1, '5CZ7421XYZ', 1);

-- Sau khi nhận máy lỗi, cập nhật lại trạng thái thiết bị vật lý sang dạng hàng lỗi chờ xử lý
UPDATE ProductItems SET Status = 'Defective' WHERE SerialNumber = '5CZ7421XYZ';