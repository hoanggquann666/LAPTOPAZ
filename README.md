# LaptopAZ — Hệ thống quản lý cửa hàng laptop

Ứng dụng desktop **WinForms** (.NET Framework 4.7.2) quản lý sản phẩm, nhập kho theo serial, bán hàng, đơn đặt trước, đổi trả và dashboard báo cáo. Giao diện thương hiệu **Azure Management**, sidebar tối và vùng nội dung sáng.

---

## Yêu cầu hệ thống

| Thành phần | Phiên bản / ghi chú |
|------------|---------------------|
| Windows | 10 trở lên |
| Visual Studio | **2022 17.10+** (bắt buộc mở file `.slnx`) |
| .NET Framework | **4.7.2** Targeting Pack |
| SQL Server | **Express LocalDB** — `(localdb)\MSSQLLocalDB` |
| Workload VS | .NET desktop development |

---

## Cấu trúc solution

```
LAPTOPAZ/                          ← Thư mục gốc (clone)
├── LaptopAZ.UI/                   ← WinForms — điểm vào ứng dụng
│   ├── LaptopAZ.UI.slnx           ← Mở file này trong Visual Studio
│   ├── MainForm.cs                ← Toàn bộ view (Dashboard, Sales, …)
│   ├── LoginForm.cs
│   └── App.config                 ← Connection string
├── LaptopAZ.BLL/                  ← Nghiệp vụ (Auth, Sales, Warehouse, Dashboard, …)
├── LaptopAZ.DAL/                  ← DbContext (Entity Framework 6)
├── LaptopAZ.Repository/           ← Generic Repository + Unit of Work
├── LaptopAZ.Models/               ← Entity ánh xạ bảng SQL
├── LaptopAZ.DTO/                  ← DTO truyền giữa các tầng
├── LaptopAZ.Helpers/              ← SessionHelper, RolePermissions, PasswordHelper
├── SQL/
│   ├── LaptopAZDatabase.sql       ← Schema + seed (chạy tự động lần đầu)
│   └── Patches/                   ← Script bổ sung (nếu có)
├── README.md                      ← Tài liệu này (hướng dẫn source)
└── HUONGDANSUDUNG.md              ← Hướng dẫn người dùng cuối
```

### Luồng phụ thuộc tầng

```
LaptopAZ.UI → LaptopAZ.BLL → LaptopAZ.Repository → LaptopAZ.DAL → SQL Server
                ↘ LaptopAZ.DTO, LaptopAZ.Helpers, LaptopAZ.Models
```

---

## Công nghệ chính

| Thư viện | Mục đích |
|----------|----------|
| Entity Framework 6.4.4 | ORM, truy vấn LINQ |
| BCrypt.Net-Next | Mã hóa / xác thực mật khẩu |
| Guna.UI2.WinForms | Nút, ô nhập bo góc |
| LiveCharts.WinForms | Biểu đồ xu hướng Dashboard |
| ADO.NET (trong BLL) | `DapperReportService` — báo cáo doanh thu |

---

## Chạy nhanh (lần đầu)

### 1. Clone và mở solution

```bash
git clone <url-repo-cua-ban> LAPTOPAZ
```

Mở bằng Visual Studio 2022 (17.10+):

```
LaptopAZ.UI\LaptopAZ.UI.slnx
```

### 2. Restore & startup project

- **Restore NuGet Packages** (chuột phải Solution nếu cần).
- Chuột phải **LaptopAZ.UI** → **Set as Startup Project**.

### 3. Build & chạy

**Visual Studio:** F5  

**Dòng lệnh:**

```bash
cd LaptopAZ.UI
dotnet build LaptopAZ.UI.csproj -c Debug
dotnet run --project LaptopAZ.UI.csproj
```

*(Cần .NET SDK hỗ trợ `net472` để `dotnet build`.)*

### 4. Khởi tạo CSDL tự động

`Program.cs` → `InitializeDatabase()`:

1. Kiểm tra database `LaptopAZDB` trên LocalDB.
2. Nếu chưa có → `CREATE DATABASE` → thực thi `SQL\LaptopAZDatabase.sql` (tách lệnh theo `GO`).
3. Hiển thị hộp thoại thành công → mở **LoginForm**.

**Không cần** chạy script SQL thủ công trên máy mới (trừ khi tự sửa DB).

---

## Tài khoản mẫu

| Vai trò | Username | Password |
|--------|----------|----------|
| Admin | `tuan.leadmin` | `admin` |
| Nhân viên kho | `mai.nguyenkho` | `kho` |
| Nhân viên bán hàng | `nam.tranbanhang` | `banhang` |
| Kế toán | `linh.ketoan` | `ketoan` |

Hash trong seed dùng định dạng mock; `PasswordHelper.VerifyPassword` hỗ trợ BCrypt và fallback cho mật khẩu mẫu trên.

---

## Cấu hình kết nối

File: `LaptopAZ.UI\App.config`

```xml
<connectionStrings>
  <add name="LaptopAZDbContext"
       connectionString="Server=(localdb)\MSSQLLocalDB;Database=LaptopAZDB;Trusted_Connection=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

Đổi `Server=` nếu dùng SQL Server Express/full instance (ví dụ `localhost\SQLEXPRESS`). Ứng dụng vẫn tự tạo DB nếu chưa tồn tại (khi có quyền).

---

## Tầng nghiệp vụ (BLL) — tham khảo

| Service | Trách nhiệm chính |
|---------|-------------------|
| `AuthService` | Đăng nhập, danh sách role |
| `ProductService` | CRUD sản phẩm, khách hàng, hãng, danh mục |
| `WarehouseService` | Phiếu nhập, serial `ProductItem`, tồn kho |
| `SalesService` | Đơn `HD-`/`DH-`, trạng thái, thanh toán, xóa dòng/serial |
| `ReturnService` | Đổi trả |
| `DashboardService` | Thống kê EF cho Dashboard |
| `DapperReportService` | Doanh thu / aggregate SQL trực tiếp |

Phân quyền tập trung: `LaptopAZ.Helpers/RolePermissions.cs` — UI gọi `CanAccessTab`, `IsViewOnly`, `CanManageProducts`, …

---

## UI động (MainForm)

Không tách từng UserControl theo module; `MainForm.cs` dựng giao diện theo tab:

| Tab (Designer) | Hàm view |
|----------------|----------|
| Dashboard | `ShowDashboardView()` — **4 KPI** + cảnh báo tồn + xu hướng |
| Sản phẩm | `ShowProductsView()` |
| Hãng & Danh mục | `ShowCategoriesView()` |
| Nhập kho | `ShowImportView()` |
| Bán hàng | `ShowSalesView()` |
| Quản lý đơn | `ShowOrderManagementView()` |
| Trả hàng | `ShowReturnsView()` |
| Khách hàng \| Đối tác | `ShowPartnersView()` |
| Nhân viên | `ShowStaffView()` |

Scaling DPI: hàm `scale()` trong `MainForm` (theo `DeviceDpi / 96`).

---

## Quy ước nghiệp vụ quan trọng (đọc code)

- **Serial:** Mỗi máy một `ProductItem.SerialNumber`; trạng thái `InStock` → `Reserved` (đặt hàng) → `Sold` (bán).
- **Mã đơn:** `HD-` = bán tại quầy (`Paid` ngay); `DH-` = đặt trước (`Pending`, xử lý tại Quản lý đơn).
- **Admin:** Không CRUD sản phẩm (`CanManageProducts == false`), vẫn quản lý hãng/danh mục và vận hành đơn.
- **Kế toán:** `IsViewOnly` — chặn mutation ở UI và `EnsureCanMutateBusinessData()` ở BLL.

Chi tiết thao tác màn hình: **HUONGDANSUDUNG.md**.

---

## Khắc phục sự cố

| Lỗi | Cách xử lý |
|-----|------------|
| Không mở được `.slnx` | Cập nhật VS 2022 lên **17.10+** |
| `Cannot open database "LaptopAZDB"` | SSMS: `DROP DATABASE LaptopAZDB;` → chạy lại F5 |
| Không tìm thấy `LaptopAZDatabase.sql` | Đặt thư mục `SQL\` ở **thư mục gốc** solution (cùng cấp `LaptopAZ.UI`) |
| Thiếu .NET 4.7.2 | Cài **.NET Framework 4.7.2 Targeting Pack** trong VS Installer |
| NuGet lỗi | Restore Packages; xóa `bin`/`obj` và build lại |
| UI cắt chữ / viền | Windows Display Scale **100%**; hoặc kiểm tra `scale()` trên màn hình DPI cao |
| LocalDB không chạy | Cài **SQL Server Express LocalDB**; kiểm tra instance `(localdb)\MSSQLLocalDB` |

---

## Phát triển & đóng góp

1. Tạo nhánh feature từ `main`.
2. Sửa đúng tầng: UI chỉ điều hướng; logic nghiệp vụ trong BLL; không truy cập `DbContext` trực tiếp từ UI.
3. Thay đổi schema: cập nhật `SQL\LaptopAZDatabase.sql` và entity tương ứng.
4. `dotnet build` trước khi commit.

---

## Tài liệu liên quan

- **[HUONGDANSUDUNG.md](./HUONGDANSUDUNG.md)** — Hướng dẫn sử dụng cho nhân viên / quản trị
- **SQL/LaptopAZDatabase.sql** — Cấu trúc bảng và dữ liệu mẫu

---

*LaptopAZ · .NET Framework 4.7.2 · Entity Framework 6 · WinForms*
