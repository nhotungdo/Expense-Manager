Dựa trên bảng `SharedAccounts` và cấu trúc Database hiện có, chức năng "Ví chia sẻ" (Shared Wallet) khác với "Chi tiêu nhóm".

* **Chi tiêu nhóm (Group Expenses):** Là ghi nợ, trả nợ sau (Giống Splitwise).
* **Ví chia sẻ (Shared Wallet):** Là **nhiều người cùng truy cập vào một nguồn tiền thực tế** (Ví dụ: Tài khoản ngân hàng chung của vợ chồng, Quỹ tiền mặt của phòng ban, Heo đất ảo).

Dưới đây là danh sách các tính năng cần thiết cho trang giao diện này:

### 1. Nhóm Chức năng Quản lý Ví (Admin/Owner Features)

Chỉ người tạo ví (`SharedByUserId` trong bảng `SharedAccounts`) mới có toàn quyền này.

* **Mời thành viên (Invite Members):**
* Tìm kiếm người dùng qua Email, Số điện thoại hoặc từ danh sách **Bạn bè** (Bảng `Friendships` đã tạo).
* Gửi thông báo mời tham gia vào bảng `Notifications`.


* **Phân quyền truy cập (Permission Settings):**
Dựa trên cột `Permission` trong bảng `SharedAccounts`, bạn cần làm giao diện Dropdown cho phép chọn:
1. **Chỉ xem (View Only - 1):** Dành cho con cái xem ví bố mẹ, hoặc nhân viên xem quỹ sếp. Chỉ xem số dư và lịch sử, không được thêm/sửa/xóa.
2. **Xem & Thêm (View & Add - 2):** Được thêm giao dịch mới, nhưng không được xóa giao dịch của người khác hoặc chỉnh sửa ví.
3. **Toàn quyền (Full Access - 3):** Ngang hàng với chủ ví (Vợ/Chồng). Được sửa, xóa, mời thêm người khác.


* **Trục xuất thành viên (Remove Member):**
* Xóa quyền truy cập của một người khỏi ví.
* Lịch sử giao dịch cũ của người đó vẫn phải được giữ lại (Không được xóa transaction).



### 2. Nhóm Chức năng Hiển thị & Thao tác (Operational Features)

* **Hiển thị Người chi tiêu (Spender Identification):**
* Trong danh sách giao dịch của ví này, cần hiển thị thêm cột hoặc icon **"Người thực hiện"**.
* *Logic:* Lấy `Avatar` và `FullName` từ bảng `Users` dựa trên `UserId` của từng record trong bảng `Transactions`.
* *UI:* Avatar nhỏ nằm cạnh số tiền.


* **Số dư thời gian thực (Real-time Sync):**
* Khi Vợ thêm giao dịch mua sắm, máy của Chồng phải cập nhật số dư ngay lập tức (Dùng SignalR hoặc cơ chế Pull-to-refresh).


* **Bộ lọc theo thành viên (Member Filter):**
* Thêm filter: "Xem giao dịch của A", "Xem giao dịch của B" hoặc "Xem tất cả".



### 3. Nhóm Chức năng Báo cáo & Minh bạch (Transparency)

Đây là yếu tố quan trọng nhất của ví chung để tránh tranh cãi.

* **Biểu đồ "Ai tiêu nhiều nhất?" (Spending Contribution):**
* Biểu đồ tròn hoặc thanh ngang so sánh tổng tiền chi tiêu của từng thành viên trong tháng.
* *Query:* `SELECT UserId, SUM(Amount) FROM Transactions WHERE AccountId = @SharedId GROUP BY UserId`.


* **Nhật ký hoạt động (Audit Log):**
* Hiển thị lịch sử thay đổi nhạy cảm: "A đã sửa giao dịch X từ 50k thành 500k", "B đã xóa giao dịch Y".
* Dữ liệu lấy từ bảng `AuditLogs` nếu bạn có ghi log các hành động Update/Delete.


* **Thông báo biến động (Alerts):**
* Gửi Push Notification/Email cho tất cả thành viên khi có giao dịch mới: *"Vợ vừa chi 200k cho Siêu thị"*.
* Cấu hình trong bảng `Notifications`.



### 4. Gợi ý Thiết kế UI (Layout)

* **Header Ví:**
* Tên ví + Tổng số dư thật to.
* Danh sách Avatar các thành viên đang tham gia (chồng lên nhau dạng Stack). Nút `+` bên cạnh để mời nhanh.


* **Tab "Tổng quan":**
* Biểu đồ đóng góp (Ai tiêu bao nhiêu).
* Biểu đồ xu hướng số dư chung.


* **Tab "Giao dịch":**
* List giao dịch thông thường nhưng có thêm Avatar người tạo.


* **Tab "Cài đặt" (Chỉ hiện cho Admin):**
* List thành viên dạng danh sách dọc. Mỗi dòng có Dropdown chỉnh quyền (View/Edit/Full) và nút Xóa (Thùng rác).
* Nút "Rời khỏi ví" (Leave Wallet) cho thành viên thường.
* Nút "Xóa ví vĩnh viễn" (Delete Wallet) cho chủ ví.



### Lưu ý Logic Backend quan trọng

Khi insert giao dịch vào ví chia sẻ, bạn phải cẩn thận với trường `UserId` trong bảng `Transactions`.

* `AccountId`: ID của ví chung.
* `UserId`: ID của **người thực hiện hành động** (người đang đăng nhập), **KHÔNG PHẢI** ID của chủ ví.
* Điều này giúp hệ thống phân biệt được ai là người đã tiêu tiền trong ví chung đó.