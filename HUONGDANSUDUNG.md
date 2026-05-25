# Hướng dẫn sử dụng LaptopAZ (Azure Management)

Tài liệu mô tả cách vận hành ứng dụng **LaptopAZ** — hệ thống quản lý cửa hàng laptop trên Windows (WinForms). Giao diện chính hiển thị thương hiệu **Azure Management** trên sidebar; nội dung từng màn hình dùng nền sáng, bảng dữ liệu và thẻ KPI dễ đọc.

---

## 1. Đăng nhập

1. Chạy ứng dụng → màn hình **Đăng nhập**.
2. Nhập **Tên đăng nhập** và **Mật khẩu** → **Đăng nhập**.
3. Sau khi đăng nhập, sidebar và các tab được hiển thị theo **vai trò** của tài khoản.

**Tài khoản mẫu (sau khi khởi tạo CSDL lần đầu):**

| Vai trò | Tên đăng nhập | Mật khẩu |
|--------|----------------|----------|
| Quản trị viên | `tuan.leadmin` | `admin` |
| Nhân viên kho | `mai.nguyenkho` | `kho` |
| Nhân viên bán hàng | `nam.tranbanhang` | `banhang` |
| Kế toán | `linh.ketoan` | `ketoan` |

> Nút **SSO Azure** trên màn hình đăng nhập hiện chỉ hiển thị thông báo bảo trì; vui lòng dùng tài khoản nội bộ.

---

## 2. Phân quyền và menu

| Vai trò | Tab hiển thị | Ghi chú |
|--------|-------------|---------|
| **Quản trị viên (Admin)** | Dashboard, Sản phẩm, Hãng & Danh mục, Nhập kho, Bán hàng, Quản lý đơn, Trả hàng, Khách hàng \| Đối tác, Nhân viên | Toàn quyền điều hành; **không** được thêm/sửa/xóa sản phẩm (chỉ xem danh sách và serial). Quản lý hãng/danh mục được phép. |
| **Nhân viên kho** | Hãng & Danh mục, Sản phẩm, Nhập kho, Khách hàng \| Đối tác | Thêm/sửa/xóa sản phẩm, nhập kho, quản lý NCC. |
| **Nhân viên bán hàng** | Bán hàng, Quản lý đơn, Trả hàng, Khách hàng \| Đối tác | Lập hóa đơn, xử lý đơn, đổi trả. |
| **Kế toán** | Dashboard, Nhập kho, Bán hàng, Quản lý đơn, Trả hàng | **Chỉ xem** — các nút thêm/sửa/xóa/thanh toán bị vô hiệu; không mở danh sách sản phẩm từ link cảnh báo tồn kho trên Dashboard. |

Header hiển thị họ tên, vai trò (tiếng Việt), đồng hồ thời gian thực và **Đăng xuất**. Chuông thông báo chỉ hiện với tài khoản Admin.

---

## 3. Dashboard (Admin & Kế toán)

Dùng để theo dõi nhanh tình hình trong ngày.

### 3.1. Bốn thẻ KPI (một hàng)

| Thẻ | Ý nghĩa |
|-----|---------|
| **Doanh thu hôm nay** | Tổng tiền các hóa đơn đã thanh toán trong ngày |
| **Hóa đơn hôm nay** | Số giao dịch bán trong ngày |
| **Cảnh báo hết hàng** | Số mẫu laptop dưới ngưỡng tồn an toàn |
| **Sản phẩm kinh doanh** | Tổng số mẫu đang quản lý |

Bốn thẻ tự giãn đều theo chiều ngang và chiều cao vùng KPI khi thay đổi kích thước cửa sổ.

### 3.2. Cảnh báo tồn kho

- Danh sách máy sắp hết / hết hàng kèm số lượng tồn.
- **Xem tất cả** (trừ Kế toán): chuyển sang tab **Sản phẩm**.

### 3.3. Xu hướng doanh thu

- Nút **Xem Chi Tiết** mở cửa sổ biểu đồ doanh thu 6 tháng gần nhất (LiveCharts).

---

## 4. Sản phẩm

**Quyền thao tác:** Nhân viên kho — đầy đủ CRUD; Admin — chỉ xem; Kế toán — không truy cập tab.

- **Tìm kiếm:** Gõ tên hoặc mã → **Tìm Kiếm**.
- **Thêm laptop:** **+ Thêm Laptop** (hoặc **+ New Inventory** trên sidebar nếu là nhân viên kho).
- **Biên tập:** Chọn dòng trong lưới → sửa form bên phải → **Cập nhật**.
- **Xóa:** Chọn sản phẩm → **Xóa sản phẩm** (chỉ khi chưa phát sinh giao dịch).
- **Xem Serial:** Xem danh sách serial theo từng máy (`InStock`, `Reserved`, `Sold`, …).

**Nhập liệu giá:** Chỉ nhập số nguyên (ví dụ `18500000`), không gõ dấu chấm/phẩy hay chữ «đ». Hệ thống tự định dạng khi hiển thị.

**Thông số kỹ thuật:** CPU, RAM, GPU, ổ cứng, màn hình — mỗi trường tối đa 255 ký tự.

---

## 5. Hãng & Danh mục

- Quản lý **Hãng** (Brand) và **Danh mục** (Category).
- Thêm hãng/danh mục trước khi tạo sản phẩm mới.
- Admin được thêm/xóa hãng & danh mục; không xóa khi còn sản phẩm liên kết.

---

## 6. Nhập kho

Nhập laptop từ nhà cung cấp, gắn **serial** từng máy, tăng tồn kho.

1. Chọn **Nhà cung cấp**.
2. Chọn **Sản phẩm**, nhập **Số lượng**, **Đơn giá nhập**.
3. Nhập danh sách **Serial** (mỗi dòng một serial, đúng số lượng).
4. **Thêm vào phiếu** → kiểm tra tổng tiền → **Hoàn tất nhập hàng**.

Có thể tra **Lịch sử nhập** và tìm theo mã phiếu / nhà cung cấp. Kế toán chỉ xem, không tạo phiếu.

---

## 7. Bán hàng (Lập hóa đơn)

### 7.1. Khách hàng

- Nhập **SĐT** → Enter hoặc rời ô → hệ thống tra cứu khách thành viên.
- Không có trong hệ thống: nhập **Tên** (bắt buộc) để tự tạo khách mới khi lưu đơn.
- Có thể nhập Email, Địa chỉ.

### 7.2. Thêm sản phẩm vào giỏ

1. Ô **Tìm Laptop:** gõ tên, mã, hãng hoặc danh mục.
2. Chọn dòng gợi ý (click hoặc phím mũi tên + Enter).
3. **+ Thêm Vào Giỏ Hàng**.
4. **Chọn Serial Cho Máy** — bắt buộc đủ serial theo số lượng từng dòng.
5. Dùng **+ SL** / **- SL** / **Xóa Dòng** để điều chỉnh giỏ.

### 7.3. Thanh toán

| Nút | Mã đơn | Trạng thái | Tồn kho |
|-----|--------|------------|---------|
| **Đặt Hàng** | `DH-...` | `Pending` — serial **Reserved** | Chưa trừ kho |
| **Xuất Hóa Đơn** | `HD-...` | `Paid` — serial **Sold** | Trừ kho ngay |

- Nhập **Giảm giá (đ)** nếu có → hệ thống tính **Tạm tính** và **Thành tiền**.
- Cột phải: **Lịch sử bán hàng** — tìm và in hóa đơn khi đơn **Hoàn thành**.

---

## 8. Quản lý đơn

Theo dõi đơn `DH-` (đặt trước) và `HD-` (bán tại quầy).

### 8.1. Lọc trạng thái

Chờ xử lý, Đã xác nhận, Đã thanh toán, Đang giao, Đã giao, Hoàn thành, Đã hủy, …

### 8.2. Quy trình đơn đặt (`DH-`)

```
Pending → [Xác nhận] → Confirmed → [Thanh toán] → Paid
       → [Đang giao] → Shipping → [Đã giao] → Delivered → [Hoàn thành] → Completed
```

- **Thanh toán:** Gán serial (ưu tiên serial đã Reserved khi đặt hàng).
- **Hoàn thành:** Cập nhật báo cáo; có thể in hóa đơn từ lịch sử bán.

### 8.3. Đơn bán tại quầy (`HD-`)

Thường đã **Paid** sau **Xuất Hóa Đơn** → **Hoàn thành** để kết thúc và in.

### 8.4. Xóa mặt hàng khỏi đơn

- Chỉ khi đơn **Chờ xử lý (Pending)**.
- **Xóa mặt hàng** → chọn serial cần gỡ → xác nhận.
- Tổng tiền, giảm giá, thành tiền được tính lại tự động.

### 8.5. Hủy đơn

**Hủy đơn** — hoàn kho/serial tùy trạng thái đơn (theo quy tắc nghiệp vụ hệ thống).

---

## 9. Trả hàng

1. Tìm hóa đơn (mã đơn / SĐT khách).
2. Chọn serial cần trả.
3. Nhập **Lý do trả**.
4. **Xác nhận trả hàng**.

Hệ thống cập nhật kho và báo cáo theo nghiệp vụ đổi trả.

---

## 10. Khách hàng | Đối tác

Hai loại trên cùng một màn hình:

- **Khách hàng:** Người mua laptop.
- **Nhà cung cấp:** Đối tác nhập hàng.

Thêm/sửa: tên, SĐT, email, địa chỉ → **Lưu**. Admin, nhân viên kho và bán hàng dùng theo phạm vi tab được cấp.

---

## 11. Nhân viên (chỉ Admin)

- Tạo tài khoản: Họ tên, Username, Mật khẩu, **Vai trò** (Quản trị viên / Nhân viên kho / Nhân viên bán hàng / Kế toán).
- **Tạo tài khoản** — nhân viên đăng nhập với quyền tương ứng.

---

## 12. Mẹo vận hành

| Nội dung | Khuyến nghị |
|----------|-------------|
| Serial | Mỗi laptop một serial duy nhất; luôn chọn serial trước khi **Đặt Hàng** hoặc **Xuất Hóa Đơn** |
| Đơn đặt vs bán ngay | `DH-` = giữ hàng, xử lý sau tại **Quản lý đơn**; `HD-` = bán và trừ kho ngay |
| In hóa đơn | Chỉ khi trạng thái **Hoàn thành** |
| Hiển thị màn hình | Nên để Windows Scale **100%** nếu chữ/viền bị cắt trên màn 4K |
| Bảo mật | Không dùng chung tài khoản giữa các vai trò |

---

## 13. Xử lý sự cố thường gặp

| Triệu chứng | Gợi ý |
|-------------|--------|
| Không đăng nhập được | Kiểm tra username/password; đảm bảo CSDL đã được khởi tạo |
| «Không đủ tồn kho» | Kiểm tra serial còn `InStock`; nhập thêm hàng tại **Nhập kho** |
| «Chưa chọn đủ serial» | Mở **Chọn Serial Cho Máy** cho từng dòng giỏ |
| Không xóa được mặt hàng đơn | Đơn phải ở **Chờ xử lý** |
| Kế toán không thao tác được | Đúng thiết kế — chỉ xem báo cáo |

Chi tiết cài đặt môi trường, build source và khắc phục lỗi kỹ thuật: xem **README.md**.

---

*LaptopAZ — Azure Management · Phiên bản tài liệu cập nhật theo giao diện 4 KPI Dashboard và luồng bán hàng DH-/HD-.*
