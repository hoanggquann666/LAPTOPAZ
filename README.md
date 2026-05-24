# 💻 LaptopAZ - Hệ Thống Quản Lý Cửa Hàng Laptop Cục Bộ

Dự án **LaptopAZ** là hệ thống quản trị nội bộ cho chuỗi cửa hàng bán lẻ Laptop chuyên nghiệp. Dự án được thiết kế chuẩn cấu trúc công nghiệp 3 tầng (3-Tier/Layered Architecture) trên nền tảng **C# .NET Framework 4.7.2** kết hợp **Entity Framework 6** và **MS SQL Server LocalDB**.

Phiên bản hiện tại đã được nâng cấp toàn diện về **Trải nghiệm giao diện cao cấp (Premium Dark Theme)** và tích hợp **Cơ chế tự sửa đổi/tự khởi tạo CSDL cục bộ (Self-Healing Database)** giúp làm việc nhóm vô cùng dễ dàng.

---

## 🚀 ĐIỂM NỔI BẬT SAU NÂNG CẤP

### 1. Tự động hóa kết nối CSDL (Zero-Config Database)
- **Không cần cài đặt thủ công**: Hệ thống tự động kiểm tra sự tồn tại của database `LaptopAZDB` khi khởi chạy lần đầu.
- **Tự động sinh cấu trúc & nạp dữ liệu (Auto Seeding)**: Nếu chưa có database, chương trình sẽ tự tạo DB trên SQL Server LocalDB (`(localdb)\MSSQLLocalDB`) và tự động thực thi tệp kịch bản `SQL\LaptopAZDatabase.sql` để xây dựng cấu trúc bảng 3NF hoàn chỉnh cùng đầy đủ dữ liệu mẫu (sản phẩm, tài khoản nhân viên, hóa đơn, lịch sử kho,...). 
- **100% Sẵn sàng chạy**: Mọi thành viên trong nhóm chỉ cần tải code về, mở Visual Studio và nhấn **F5** để chạy ngay.

### 2. Giao diện tối cao cấp (Premium Dark Theme & Fluid UX)
- **Tông màu Midnight Deep Slate**: Sử dụng bảng màu Slate hiện đại (`#0B0F19`, `#111827`, `#1E293B`) kết hợp ánh sáng Indigo hoàng gia (`#6366F1`) mang lại chiều sâu như các ứng dụng SaaS hiện đại.
- **Card-Layout Glassmorphic**: Mọi bảng điều khiển và vùng chứa đều tự vẽ viền Slate siêu mảnh (`#2C374E`) giúp tạo chiều sâu thị giác.
- **Premium Grid & Alternating Rows**: Các bảng dữ liệu (`DataGridView`) được định cấu hình đổi màu dòng xen kẽ tinh tế, giãn độ cao dòng thoáng đãng, và dòng chọn màu xanh Indigo quý phái.
- **Micro-Animations & Hover Effects**: Các nút bấm có khả năng tự động tính toán làm sáng màu nền 15% khi rê chuột (`MouseOver`) và làm tối 10% khi nhấp chuột (`MouseDown`) thông qua thư viện vẽ động.
- **Sidebar thông minh**: Menu Sidebar tự bôi tối các tab không sử dụng và làm nổi bật màu trắng tinh khiết cho tab active cùng thanh chỉ thị Indigo di chuyển mượt mà.

---

## 🛠️ CẤU TRÚC THƯ MỤC DỰ ÁN

Giải pháp được phân rã thành nhiều Project tương ứng với mô hình 3 tầng chuẩn chỉnh:
```
📂 LaptopAZ
 ┣ 📂 LaptopAZ.UI          # [Tầng Giao Diện] Chứa Forms, Views, Logic điều khiển UI (WinForms)
 ┣ 📂 LaptopAZ.BLL         # [Tầng Nghiệp Vụ] Xử lý logic nghiệp vụ, tính toán doanh thu, tồn kho
 ┣ 📂 LaptopAZ.DAL         # [Tầng Truy Cập Dữ Liệu] Chứa Entity Framework DbContext
 ┣ 📂 LaptopAZ.Repository  # [Mô hình Repository & Unit of Work] Đảm bảo tính toàn vẹn giao dịch CSDL
 ┣ 📂 LaptopAZ.DTO         # [Data Transfer Objects] Định dạng dữ liệu vận chuyển giữa các tầng
 ┣ 📂 LaptopAZ.Models      # [Thực Thể] Lớp ánh xạ cấu trúc bảng CSDL (Entities)
 ┣ 📂 LaptopAZ.Helpers     # [Tiện Ích] Mã hóa mật khẩu BCrypt, quản lý phiên làm việc SessionHelper
 ┗ 📂 SQL                  # Chứa tệp kịch bản khởi tạo database LaptopAZDatabase.sql
```

---

## ⚙️ HƯỚNG DẪN CÀI ĐẶT CHI TIẾT (CHO THÀNH VIÊN TRONG NHÓM)

Để dự án hoạt động trơn tru nhất trên máy tính cá nhân của các thành viên trong nhóm, vui lòng thực hiện tuần tự các bước sau:

### Bước 1: Chuẩn bị môi trường trong Visual Studio
1. Mở phần mềm **Visual Studio Installer** trên máy của bạn.
2. Tại phiên bản Visual Studio đang sử dụng (2019 hoặc 2022), chọn **Modify**.
3. Tại tab **Workloads**, hãy tích chọn:
   - **.NET desktop development** (Phát triển ứng dụng máy tính .NET).
4. Tại tab **Individual components** (Thành phần riêng lẻ), hãy tìm kiếm và tích chọn:
   - **.NET Framework 4.7.2 Targeting Pack** (phiên bản SDK mục tiêu của dự án).
   - **SQL Server Express LocalDB** (máy chủ SQL Server cục bộ siêu nhẹ).
5. Nhấn **Modify** ở góc dưới cùng bên phải để tiến hành tải và cài đặt tự động.

### Bước 2: Tải code và khôi phục thư viện (Restore NuGet)
1. Tải toàn bộ thư mục code dự án về máy tính của bạn.
2. Dùng Visual Studio mở file giải pháp **`LaptopAZ.UI.sln`** (hoặc tệp `.slnx`).
3. Visual Studio sẽ tự động khôi phục các thư viện NuGet bị thiếu (như Entity Framework 6.4.4, BCrypt,...).
4. Nếu Visual Studio không tự chạy, hãy click chuột phải vào dòng **`Solution 'LaptopAZ'`** ở cột *Solution Explorer* phía bên phải -> Chọn **Restore NuGet Packages**.

### Bước 3: Thiết lập Dự án Chạy Mặc định
1. Tại cột *Solution Explorer* phía bên phải, click chuột phải vào thư mục dự án giao diện: **`LaptopAZ.UI`**.
2. Chọn **Set as Startup Project** (Thiết lập làm dự án khởi động mặc định).
3. Đảm bảo thanh công cụ phía trên hiển thị nút chạy màu xanh lá cây với chữ: **`LaptopAZ.UI`**.

### Bước 4: Chạy ứng dụng và Tận hưởng
1. Bấm nút **Start** (mũi tên xanh lá cây) hoặc nhấn phím **F5** trên bàn phím.
2. Ứng dụng sẽ tự động phát hiện máy chưa có database `LaptopAZDB` -> tiến hành tạo database mới tinh cục bộ -> thực thi nạp toàn bộ cấu trúc bảng và dữ liệu mẫu có sẵn từ file SQL.
3. Khi màn hình **ĐĂNG NHẬP** hiện ra, bạn đã hoàn tất quá trình cài đặt!

---

## 🔑 TÀI KHOẢN MẪU KHỞI CHẠY (TESTING CREDENTIALS)

CSDL mẫu đi kèm chứa 3 tài khoản tương ứng với 3 vai trò phân quyền nghiệp vụ sâu sắc của phần mềm:

| Vai Trò Phân Quyền | Tên Đăng Nhập | Mật Khẩu Đăng Nhập | Mô Tả Quyền Hạn |
| :--- | :--- | :--- | :--- |
| **Quản trị viên (Admin)** | `tuan.leadmin` | **`admin`** | Xem Dashboard doanh thu, quản lý tất cả danh mục, sản phẩm, nhân viên, bán hàng, đổi trả... |
| **Nhân viên Kho** | `mai.nguyenkho` | **`kho`** | Chỉ xem và thao tác phần quản lý Sản phẩm, Hãng & Danh mục, Phiếu Nhập Kho, Đối tác nhà cung cấp. |
| **Nhân viên Bán hàng** | `nam.tranbanhang` | **`banhang`** | Chỉ xem và thao tác Lập Hóa đơn bán hàng, Quản lý Đổi trả cho khách hàng, thông tin Khách hàng. |

---

## 💡 CẤU HÌNH NÂNG CAO (KHI CẦN THIẾT)

### Chuỗi kết nối Database cục bộ
Dự án được định vị chuỗi kết nối chuẩn trong [App.config](file:///c:/Users/Lenovo/Desktop/CodeSave/C%23/LaptopAZ.UI/App.config) của dự án `LaptopAZ.UI`:
```xml
<connectionStrings>
  <add name="LaptopAZDbContext" 
       connectionString="Server=(localdb)\MSSQLLocalDB;Database=LaptopAZDB;Trusted_Connection=True;" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```
Nếu bạn muốn sử dụng máy chủ SQL Server đầy đủ chạy trên nền dịch vụ (Service) thay vì LocalDB mặc định, bạn chỉ cần mở file [App.config](file:///c:/Users/Lenovo/Desktop/CodeSave/C%23/LaptopAZ.UI/App.config) và đổi thuộc tính `Server=(localdb)\MSSQLLocalDB` thành tên server của bạn (Ví dụ: `Server=localhost\SQLEXPRESS` hoặc `Server=.`). Chương trình vẫn sẽ tự phát hiện CSDL thiếu và khởi tạo CSDL mới hoàn toàn tương tự!

---

## 🛡️ KHẮC PHỤC LỖI THƯỜNG GẶP (TROUBLESHOOTING)

1. **Lỗi `Cannot open database "LaptopAZDB" requested by the login.`**:
   - *Nguyên nhân*: Do CSDL bị lỗi đồng bộ quyền truy cập.
   - *Cách sửa*: Chạy lệnh `DROP DATABASE LaptopAZDB;` trên SQL Server Management Studio rồi chạy lại phần mềm F5 để nó tự tạo lại DB sạch.
2. **Lỗi thiếu gói .NET Target Framework**:
   - *Nguyên nhân*: Chưa cài đặt gói Target Pack 4.7.2.
   - *Cách sửa*: Quay lại **Bước 1** cài đặt thành phần mục tiêu bằng Visual Studio Installer.
3. **Giao diện bị nhòe / Chữ bị tràn viền (DPI Error)**:
   - *Nguyên nhân*: WinForms hiển thị kém trên màn hình 4K hoặc Scale màn hình Windows > 100%.
   - *Cách sửa*: Click chuột phải vào màn hình Desktop -> *Display settings* -> Đặt mục *Scale and layout* về `100%` để có trải nghiệm hiển thị chuẩn xác nhất.

Chúc bạn và đội ngũ của mình có những giây phút lập trình và vận hành hệ thống tuyệt vời cùng **LaptopAZ**!
