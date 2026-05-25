-- Patch ghi chú: ProductItems.Status hỗ trợ thêm giá trị 'Reserved'
-- (đơn đặt hàng giữ serial trước khi thanh toán).
-- Không cần ALTER TABLE vì cột Status đã là VARCHAR(30).

-- Giá trị hợp lệ: InStock | Reserved | Sold | Warranty | Defective
