# 📈 HƯỚNG DẪN SỬ DỤNG HỆ THỐNG QUẢN LÝ LAPTOPAZ
*(Hệ thống Quản trị Azure Management - Chuyên nghiệp & Hiện đại)*

Chào mừng bạn đến với tài liệu hướng dẫn sử dụng hệ thống **LaptopAZ**. Tài liệu này cung cấp toàn bộ hướng dẫn thao tác chi tiết, các quy trình quản lý nghiệp vụ và phân quyền sử dụng hệ thống cho các vai trò khác nhau.

---

## 🔑 1. PHÂN QUYỀN HỆ THỐNG (AUTHORIZATION)
Hệ thống được thiết kế để phân chia quyền hạn rõ rệt dựa trên vai trò làm việc của từng nhân viên nhằm tối ưu hóa bảo mật và chuyên môn hóa công việc:

### 👤 Quản trị viên (Admin)
* **Quyền hạn**: Toàn quyền truy cập tất cả các chức năng của hệ thống.
* **Các tab hiển thị**: `Dashboard`, `Sản phẩm`, `Hãng | Danh mục`, `Nhập hàng`, `Bán hàng`, `Trả hàng`, `Khách hàng | Đối tác`, `Nhân viên`.

### 📦 Nhân viên Kho (WarehouseStaff)
* **Quyền hạn**: Quản lý xuất nhập kho, cấu hình sản phẩm và nhà cung cấp.
* **Các tab hiển thị**:
  1. `📁 Hãng | Danh mục`: Quản lý các hãng laptop.
  2. `💻 Danh sách Sản phẩm`: Thêm mới, chỉnh sửa thông số laptop trong kho.
  3. `📥 Nhập hàng`: Nhập thêm số lượng từ các nhà cung cấp.
  4. `👥 Khách hàng | Đối tác`: Quản lý thông tin nhà cung cấp sản phẩm.

### 💰 Nhân viên Bán hàng (SalesStaff)
* **Quyền hạn**: Lập hóa đơn bán hàng cho khách hàng, tiếp nhận đổi trả sản phẩm lỗi.
* **Các tab hiển thị**:
  1. `💵 Bán hàng`: Tạo đơn hàng và thanh toán trực tiếp cho khách.
  2. `🔄 Trả hàng`: Xử lý đổi trả hàng hóa từ người mua.
  3. `👥 Khách hàng | Đối tác`: Tạo và tra cứu thông tin khách hàng mua máy.

---

## 📊 2. CHI TIẾT CÁC CHỨC NĂNG & THAO TÁC THỰC HIỆN

### 2.1. TRANG CHỦ & PHÂN TÍCH (DASHBOARD)
Dành riêng cho **Admin** để theo dõi hiệu suất kinh doanh trong ngày và xu hướng doanh thu.

* **Các thẻ chỉ số KPI**:
  * **Doanh thu hôm nay**: Tổng số tiền thu được từ các hóa đơn thanh toán trong ngày hiện tại.
  * **Hóa đơn hôm nay**: Tổng số giao dịch xuất hóa đơn thành công.
  * **Cảnh báo hết hàng**: Số lượng laptop có lượng tồn dưới định mức an toàn (sắp hết hoặc hết hàng).
  * **Sản phẩm kinh doanh**: Tổng số mẫu laptop đang được quản lý trên phần mềm.
* **Cảnh báo tồn kho & Gợi ý quản trị**:
  * Liệt kê các dòng máy sắp hết kèm số lượng tồn cụ thể để thủ kho/admin kịp thời nhập hàng.
  * Đưa ra các gợi ý thông minh dựa trên xu hướng thị trường (ví dụ: bổ sung Dell XPS vào mùa tựu trường).
* **Báo cáo xu hướng**:
  * Nhấn nút **"Xem báo cáo chi tiết"**: Hệ thống sẽ mở ra một cửa sổ popup hiển thị biểu đồ doanh thu dạng cột (gradient màu xanh lam - tím) trực quan hóa doanh thu 6 tháng gần nhất để phục vụ công tác hoạch định chiến lược.

---

### 2.2. QUẢN LÝ SẢN PHẨM (INVENTORY)
Nơi cập nhật thông tin chi tiết và kiểm kê số lượng laptop trong cửa hàng.

* **Tìm kiếm sản phẩm**: Sử dụng thanh tìm kiếm ngay trong phần nội dung trang Sản phẩm, gõ tên máy hoặc mã sản phẩm và nhấn nút tìm kiếm.
* **Thêm sản phẩm mới**:
  1. Click chọn nút **`+ New Inventory`** ở menu sidebar hoặc nút **`Thêm mới`** trong danh sách.
  2. Điền đầy đủ thông tin vào Form biên tập bên phải:
     * *Tên sản phẩm* (Ví dụ: Acer Swift Go 14).
     * *Hãng sản xuất* (Chọn từ danh sách thả xuống, ví dụ: Acer).
     * *Giá nhập & Giá bán*: Nhập số nguyên dương (Ví dụ: `15000000`, **không nhập dấu chấm, phẩy hay chữ "đ"**).
     * *Số lượng tồn*: Số lượng máy có sẵn ban đầu.
     * *Thông số kỹ thuật*: CPU, RAM, GPU, Ổ cứng, Màn hình. *(Hệ thống hiện tại hỗ trợ nhập dữ liệu cấu hình cực kỳ chi tiết lên tới **255 ký tự** cho mỗi trường mà không bị lỗi)*.
  3. Bấm **`Thêm`** để lưu trữ.
* **Chỉnh sửa / Xóa sản phẩm**:
  * Chọn sản phẩm cần sửa trong bảng danh sách -> Thông tin sản phẩm tự động hiển thị lên form biên tập -> Sửa các thông số cần thiết -> Bấm **`Cập nhật`**.
  * Để xóa, chọn sản phẩm và bấm nút **`Xóa`** (Chỉ xóa được sản phẩm chưa phát sinh giao dịch bán/nhập hàng để đảm bảo tính toàn vẹn dữ liệu).

---

### 2.3. HÃNG | DANH MỤC (BRANDS)
Quản lý các thương hiệu Laptop hợp tác kinh doanh. Bạn phải thêm hãng ở đây trước khi tạo sản phẩm thuộc hãng đó.

* **Thêm hãng**: Gõ tên hãng (ví dụ: *Asus, Acer, Dell, HP*) vào ô nhập liệu -> Bấm **`Thêm hãng`**.
* **Xóa hãng**: Chọn hãng cần xóa từ danh sách và bấm **`Xóa`** (Không được xóa hãng đang có sản phẩm liên kết).

---

### 2.4. NHẬP HÀNG (GOODS IMPORT)
Nghiệp vụ nhập thêm máy từ Nhà cung cấp vào kho để tăng số lượng tồn.

* **Tạo đơn nhập hàng**:
  1. Chọn **Nhà cung cấp** từ danh sách đối tác đã lưu.
  2. Chọn **Laptop** cần nhập và điền **Số lượng nhập**, **Đơn giá nhập**.
  3. Bấm **`Thêm vào danh sách nhập`** để lập danh sách tạm thời.
  4. Xác nhận tổng tiền và bấm **`Hoàn tất Nhập hàng`** để cập nhật số lượng tồn kho tự động tăng lên trên hệ thống.

---

### 2.5. BÁN HÀNG (SALES POINT)
Giao diện lập hóa đơn bán hàng cho khách hàng nhanh chóng, chuyên nghiệp.

* **Quy trình bán hàng**:
  1. Điền thông tin khách hàng hoặc chọn khách hàng thành viên có sẵn trên hệ thống.
  2. Tại bảng danh sách laptop, chọn sản phẩm khách hàng muốn mua.
  3. Nhập số lượng mua (hệ thống sẽ tự động chặn nếu số lượng bán vượt quá số lượng tồn kho thực tế).
  4. Nhấn **`Thêm sản phẩm`** để đưa vào giỏ hàng. Hệ thống tự động tính tổng tiền và mức chiết khấu (nếu có).
  5. Bấm **`Thanh toán & In hóa đơn`** để xác nhận thanh toán. Số lượng tồn kho sản phẩm sẽ tự động giảm đi tương ứng.

---

### 2.6. TRẢ HÀNG (RETURNS & REFUNDS)
Giải quyết các trường hợp đổi trả sản phẩm bị lỗi kỹ thuật hoặc theo mong muốn của khách hàng.

* **Quy trình trả hàng**:
  1. Tra cứu hóa đơn bán hàng ban đầu qua ô tìm kiếm mã hóa đơn.
  2. Chọn sản phẩm khách muốn trả lại trong danh sách hóa đơn đó.
  3. Nhập lý do trả hàng (ví dụ: *Máy lỗi màn hình*, *Khách đổi ý*...).
  4. Xác nhận số tiền hoàn trả cho khách và bấm **`Xác nhận Trả hàng`**.
  5. Phần mềm tự động cập nhật sản phẩm lỗi vào khu vực chờ xử lý và hoàn trả lại tiền tương ứng trên hệ thống báo cáo.

---

### 2.7. KHÁCH HÀNG | ĐỐI TÁC (PARTNERS)
Quản lý tập trung toàn bộ dữ liệu Đối tác và Khách hàng mua sắm để phục vụ chăm sóc khách hàng và công nợ nhà cung cấp.

* **Phân loại đối tác**:
  * **Khách hàng (Customer)**: Người mua sản phẩm. Hệ thống ghi nhận lịch sử mua sắm để xếp hạng thành viên.
  * **Nhà cung cấp (Supplier)**: Đơn vị phân phối laptop cho cửa hàng.
* **Thao tác**: Thêm tên đối tác, số điện thoại, email, địa chỉ và nhấn **`Lưu`**. Bạn có thể chỉnh sửa thông tin liên lạc bất cứ lúc nào.

---

### 2.8. QUẢN LÝ NHÂN VIÊN (STAFF CONTROL)
Dành riêng cho **Admin** nhằm quản trị nhân lực và cấp tài khoản làm việc.

* **Quy trình cấp tài khoản**:
  1. Nhập **Họ tên**, **Tên đăng nhập (Username)**, **Mật khẩu** của nhân viên mới.
  2. Tại mục **Quyền hạn (Role)**: Chọn vai trò tương ứng gồm `Quản trị viên (Admin)`, `Nhân viên Kho (WarehouseStaff)` hoặc `Nhân viên Bán hàng (SalesStaff)`.
  3. Bấm **`Tạo tài khoản`**. Nhân viên mới giờ đây có thể đăng nhập bằng tài khoản này và giao diện sẽ tự động cấu hình quyền hạn chuẩn theo vai trò được gán.

---

## 💡 3. MỘT SỐ MẸO & LƯU Ý QUAN TRỌNG KHI SỬ DỤNG
* **Quy tắc nhập số tiền**: Trong toàn bộ hệ thống, khi nhập giá tiền (giá nhập, giá bán, số tiền hoàn trả), bạn **chỉ nhập các con số nguyên liên tiếp** (ví dụ: `18500000`). Phần mềm sẽ tự động định dạng hiển thị đẹp mắt có dấu phân cách hàng nghìn (ví dụ: `18.500.000 đ`) tại các bảng dữ liệu.
* **Không sao chép ký tự thừa khi cấu hình**: Mặc dù độ dài cấu hình đã được mở rộng lên 255 ký tự, bạn vẫn nên nhập cấu hình ngắn gọn súc tích (Ví dụ: `Ryzen 7 7735HS`, `16GB LPDDR5`, `RTX 4050 6GB`) để bảng danh sách sản phẩm hiển thị gọn gàng và đẹp mắt nhất.
* **Bảo mật tài khoản**: Các tài khoản được cấp quyền phù hợp với chuyên môn. Vui lòng khuyên nhân viên không chia sẻ tài khoản cho nhau để đảm bảo báo cáo doanh số cuối ngày của mỗi người luôn khớp chính xác tuyệt đối.
