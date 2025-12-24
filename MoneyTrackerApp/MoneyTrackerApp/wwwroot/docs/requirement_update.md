Dựa trên cấu trúc bảng `GroupExpenses`, `GroupTransactions`, `GroupTransactionSplits` và `GroupMembers`, trang giao diện **"Chi tiêu nhóm"** (tương tự Splitwise) 

Để mang lại trải nghiệm người dùng (UX) tốt nhất, tôi chia các tiện ích người dùng có thể thực hiện thành 4 nhóm chính:

### 1. Nhóm Tiện ích Tổng quan (Dashboard Nhóm)

Đây là màn hình đầu tiên khi người dùng bấm vào một nhóm cụ thể (ví dụ: "Du lịch Đà Nẵng").

* **Xem tình trạng nợ cá nhân (My Balance):**
* Hiển thị ngay trên đầu trang một thẻ (Card) lớn.
* **Trạng thái Dương (Màu xanh):** *"Bạn được trả lại 500.000đ"* (Người khác đang nợ bạn).
* **Trạng thái Âm (Màu đỏ):** *"Bạn nợ 250.000đ"* (Bạn cần trả tiền cho người khác).
* **Trạng thái Cân bằng (Màu xám):** *"Bạn không nợ ai cả"*.


* **Xem danh sách thành viên & trạng thái nợ của họ:**
* Hiển thị Avatar các thành viên trong nhóm.
* Bên cạnh mỗi thành viên hiển thị số tiền họ đang nợ nhóm hoặc nhóm nợ họ (Tính toán từ `GroupTransactionSplits`).


* **Thống kê nhanh:*
* Tổng số tiền cả nhóm đã tiêu.
* Thanh tiến trình (Progress bar) nếu nhóm có đặt ngân sách giới hạn (ví dụ: Quỹ nhóm chỉ có 10 triệu).



### 2. Nhóm Tiện ích Giao dịch (Core Features)

Đây là nơi diễn ra các hoạt động chính.

* **Thêm khoản chi tiêu mới (Add Expense):**
* Nhập số tiền và nội dung (VD: Ăn hải sản).
* **Chọn người trả tiền (Payer):** Mặc định là "Tôi", nhưng có thể chọn người khác (trường hợp bạn nhập hộ).
* **Chọn ngày:** Mặc định là hôm nay hoặc chọn lại ngày cũ.
* **Đính kèm ảnh:** Chụp hóa đơn thanh toán up lên để làm bằng chứng (Sử dụng tính năng Attachment).


* **Công cụ Chia tiền thông minh (Split Options):**
* Khi thêm khoản chi, người dùng có các tùy chọn chia tiền nâng cao (lưu vào `GroupTransactionSplits`):
* **Chia đều (Equally):** Tổng tiền / Số người.
* **Chia theo số tiền cụ thể (Exact Amount):** A chịu 50k, B chịu 100k...
* **Chia theo phần trăm (Percentages):** A chịu 60%, B chịu 40%.
* **Chia theo suất (Shares):** A ăn 2 suất, B ăn 1 suất.


* **Chức năng "Thanh toán nợ" (Settle Up):**
* Đây là tính năng quan trọng nhất để xóa nợ.
* Giao diện hiển thị gợi ý: *"Trả cho Hùng 200k?"*.
* Khi người dùng bấm "Xác nhận đã trả", hệ thống tạo một giao dịch đặc biệt để đưa số dư của 2 người về 0.



### 3. Nhóm Tiện ích Tương tác & Chi tiết (Activity & Details)

* **Dòng thời gian hoạt động (Activity Feed):**
* Danh sách các giao dịch được sắp xếp theo thời gian mới nhất.
* Mỗi dòng hiển thị rõ: *Tháng 12 - Ngày 24: Hùng đã trả 500k cho 'Tiền Taxi'*.
* Click vào từng giao dịch để xem chi tiết ai chịu bao nhiêu tiền.


* **Bình luận & Thả cảm xúc (Comments & Reactions):**
* Cho phép thành viên bình luận vào từng khoản chi (VD: *"Món này hôm đó đắt quá"*, *"Chưa tính tiền nước nhé"*).
* *Lưu ý:* Cần mở rộng bảng `Messages` hoặc tạo bảng `TransactionComments` nếu muốn làm tính năng này sâu hơn.


* **Sửa/Xóa giao dịch:**
* Cho phép người tạo hoặc Admin nhóm sửa lại số tiền nếu nhập sai.
* Hệ thống sẽ tự động tính toán lại nợ cho toàn bộ thành viên.



### 4. Nhóm Tiện ích Quản trị Nhóm (Settings)

* **Quản lý thành viên:**
* **Thêm bạn bè:** Mở danh sách từ bảng `Friendships` để thêm nhanh vào nhóm.
* **Gửi link mời:** Tạo link chia sẻ (Deep link) để mời người chưa kết bạn tham gia nhóm (người dùng bấm vào link sẽ được add vào `GroupMembers`).
* **Rời nhóm:** Chỉ cho phép rời nhóm khi số dư của người đó bằng 0 (đã trả hết nợ).


* **Xuất báo cáo (Export):**
* Xuất ra file Excel hoặc PDF chi tiết: "Ai đã tiêu gì, vào ngày nào".
* Tiện ích này rất hữu ích khi đi du lịch về cần tổng kết gửi cho mọi người.


* **Cài đặt hiển thị:**
* Đổi tên nhóm, đổi ảnh bìa/icon nhóm.
* Chọn đơn vị tiền tệ chính của nhóm (VD: Đi du lịch Thái Lan thì set là THB, hệ thống tự quy đổi ra VND nếu cần).



### Tóm tắt trải nghiệm người dùng (User Flow)

1. **Vào nhóm:** Thấy ngay mình đang nợ ai, bao nhiêu tiền.
2. **Đi ăn/chơi:** Bấm nút **(+)** to đùng -> Nhập 500k -> Chọn "Chia đều" -> Xong.
3. **Cuối chuyến đi:** Bấm **"Thanh toán nợ"** -> Chuyển khoản ngân hàng cho bạn bè -> Xác nhận trên app -> Số dư về 0.

