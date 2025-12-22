Dựa trên schema bạn cung cấp, tính năng "Chi tiêu nhóm" (Group Expenses) của bạn có cấu trúc rất giống với ứng dụng **Splitwise**. Đây là một tính năng phức tạp vì nó liên quan đến logic chia tiền và tính toán nợ.

Dưới đây là hướng dẫn triển khai chi tiết từ Backend (Logic xử lý) đến Frontend (Giao diện người dùng).

---

### PHẦN 1: BACKEND (ASP.NET Core API)

Bạn cần xử lý 3 luồng dữ liệu chính: **Tạo nhóm**, **Thêm giao dịch (Chia tiền)**, và **Tính toán số dư**.

#### 1. Data Transfer Objects (DTOs)

Bạn cần tạo các DTO để hứng dữ liệu từ Frontend gửi lên.

```csharp
// DTO tạo nhóm mới
public class CreateGroupDto {
    public string Name { get; set; }
    public string Description { get; set; }
    public List<long> MemberIds { get; set; } // Danh sách ID bạn bè được thêm vào
}

// DTO thêm giao dịch nhóm (QUAN TRỌNG)
public class CreateGroupTransactionDto {
    public long GroupId { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; } // Tổng tiền hóa đơn
    public long PaidByUserId { get; set; } // Ai là người trả tiền?
    public DateTime TransactionDate { get; set; }
    
    // Danh sách chia tiền (Ai chịu bao nhiêu)
    public List<SplitDetailDto> Splits { get; set; }
}

public class SplitDetailDto {
    public long UserId { get; set; }
    public decimal Amount { get; set; } // Số tiền người này phải chịu
}

```

#### 2. Logic Xử lý (Service Layer)

**A. Logic Thêm Giao dịch Nhóm (Split Logic)**
Khi lưu một giao dịch nhóm, bạn phải lưu vào bảng `GroupTransactions` và sau đó lưu chi tiết vào `GroupTransactionSplits`.

```csharp
public async Task AddGroupTransaction(CreateGroupTransactionDto input, long currentUserId)
{
    // 1. Tạo Transaction chính
    var transaction = new GroupTransaction {
        GroupId = input.GroupId,
        PaidByUserId = input.PaidByUserId,
        Amount = input.Amount,
        Description = input.Description,
        TransactionDate = input.TransactionDate,
        CreatedAt = DateTime.UtcNow
    };
    _context.GroupTransactions.Add(transaction);
    await _context.SaveChangesAsync(); // Save để lấy Id

    // 2. Tạo các Split (Chi tiết chia tiền)
    var splits = new List<GroupTransactionSplit>();
    foreach (var item in input.Splits)
    {
        splits.Add(new GroupTransactionSplit {
            GroupTransactionId = transaction.Id,
            UserId = item.UserId,
            Amount = item.Amount,
            // Nếu người chịu tiền cũng là người trả tiền -> Coi như đã thanh toán phần của họ
            IsPaid = (item.UserId == input.PaidByUserId) 
        });
    }
    _context.GroupTransactionSplits.AddRange(splits);
    
    // 3. (Tùy chọn) Gửi thông báo cho các thành viên
    // "A vừa thêm hóa đơn 'Ăn tối' 500k"
    
    await _context.SaveChangesAsync();
}

```

**B. Logic Tính Số Dư (Who owes whom)**
Đây là phần khó nhất. Công thức chuẩn là:
`Số dư = (Tổng tiền mình đã trả hộ) - (Tổng tiền mình đã tiêu)`

* Nếu `Số dư > 0`: Mọi người đang nợ mình.
* Nếu `Số dư < 0`: Mình đang nợ nhóm.

*SQL Query để lấy số dư thành viên trong nhóm:*

```sql
SELECT 
    u.FullName,
    u.Id AS UserId,
    -- Tổng tiền đã móc ví trả (PaidByUserId trong GroupTransactions)
    ISNULL((SELECT SUM(Amount) FROM GroupTransactions WHERE GroupId = @GroupId AND PaidByUserId = u.Id), 0) 
    - 
    -- Tổng tiền lẽ ra phải trả (Amount trong GroupTransactionSplits)
    ISNULL((SELECT SUM(Amount) FROM GroupTransactionSplits s 
            JOIN GroupTransactions t ON s.GroupTransactionId = t.Id 
            WHERE t.GroupId = @GroupId AND s.UserId = u.Id), 0) 
    AS Balance
FROM GroupMembers gm
JOIN Users u ON gm.UserId = u.Id
WHERE gm.GroupId = @GroupId;

```

---

### PHẦN 2: FRONTEND (UI/UX Design)

Giao diện cần tập trung vào sự minh bạch: Ai nợ ai và nợ bao nhiêu.

#### 1. Trang Danh sách Nhóm (Groups List)

* **Card Nhóm:** Hiển thị Tên nhóm, Số thành viên.
* **Trạng thái nợ:** Dòng text màu dưới tên nhóm.
* Màu xanh lá: *"Bạn cho vay 500k"*
* Màu đỏ: *"Bạn nợ 200k"*
* Màu xám: *"Đã thanh toán hết"*



#### 2. Trang Chi tiết Nhóm (Group Detail) - Layout Bento

Chia màn hình làm 2 phần: **Tổng quan** (Trên) và **Danh sách** (Dưới).

* **Header (Dashboard Nhóm):**
* Ảnh bìa nhóm hoặc Icon nhóm to.
* **Thẻ "Số dư của bạn":** Hiển thị to rõ số tiền bạn nợ hoặc được nợ.
* **Nút "Thanh toán nợ" (Settle Up):** Nút quan trọng nhất. Khi bấm vào, nó sẽ gợi ý: *"Trả cho Nguyễn Văn A: 50.000đ"*.


* **Danh sách Giao dịch (Timeline):**
* Mỗi item hiển thị: Ngày | Ai trả tiền | Nội dung | Số tiền.
* *Ví dụ:*
> **Tháng 12**
> 📅 20/12 - 🍔 **Ăn tối**
> **Hùng** đã trả **500.000đ**
> (Bạn nợ 125.000đ) -> Dòng này hiển thị nhỏ, màu đỏ cam.





#### 3. Màn hình "Thêm Giao dịch Mới" (Add Expense) - UI phức tạp nhất

Đây là nơi người dùng thực hiện hành động chia tiền.

* **Input 1: Số tiền & Nội dung:** (Như form giao dịch thường).
* **Input 2: Ai trả tiền? (Payer):**
* Mặc định là "Bạn".
* Dropdown cho phép chọn thành viên khác trong nhóm (trường hợp bạn nhập hộ).


* **Input 3: Chia cho ai? (Splitting Options):**
* Mặc định: **Chia đều (Split Equally)**. Tự động lấy Tổng tiền / Số người được chọn.
* Tab mở rộng: **Chia theo số tiền cụ thể** hoặc **Phần trăm**.
* *UI:* Danh sách Avatar các thành viên với checkbox bên cạnh.



#### 4. Chức năng "Thanh toán nợ" (Settle Up)

Khi người dùng A trả tiền mặt cho người dùng B để xóa nợ.

* Thực chất đây là một Giao dịch nhóm đặc biệt.
* **Người trả:** Người A.
* **Người nhận:** Người B (Người chi tiêu duy nhất trong giao dịch này).
* **Số tiền:** Số tiền trả nợ.
* Khi tạo giao dịch này, số dư của A sẽ tăng lên (bớt âm), số dư của B sẽ giảm đi (bớt dương) -> Cân bằng về 0.

### Kịch bản UI Flow mẫu (User Story)

1. **Tạo nhóm:** Bạn vào tab "Nhóm" -> Bấm "+" -> Chọn "Nhóm du lịch Đà Lạt" -> Chọn list bạn bè từ danh bạ (dùng bảng `Friendships` đã tạo) -> "Tạo".
2. **Đi ăn:** Bạn trả tiền bữa lẩu 1 triệu cho 4 người.
3. **Nhập liệu:** Bấm vào nhóm -> "Thêm chi tiêu" -> Nhập 1.000.000 -> Chọn "Chia đều tất cả" -> Lưu.
4. **Kết quả:**
* Số dư của bạn: +750k (Vì bạn trả 1tr, nhưng bạn chỉ tiêu 250k thực tế, 3 người kia nợ bạn 750k).
* Số dư người khác: -250k (Hiển thị đỏ trên máy họ).


5. **Trả nợ:** Bạn B đưa bạn 250k tiền mặt.
6. **Ghi nhận:** Bạn hoặc B bấm "Thanh toán nợ" -> Chọn "B trả cho Bạn: 250k" -> Lưu.
* Số dư của bạn giảm còn +500k.
* Số dư của B về 0 (Sạch nợ).



### Điểm cần lưu ý

* **Trigger cập nhật:** Bạn không cần trigger cập nhật số dư ví cá nhân (bảng `Accounts`) ngay lập tức khi thêm giao dịch nhóm, vì tiền nhóm thường là tiền "ảo" (ghi nợ). Chỉ khi nào thực hiện "Thanh toán nợ" (Settle Up) hoặc người dùng chọn "Ghi vào ví cá nhân", lúc đó mới trừ tiền thật trong bảng `Accounts` và tạo record trong bảng `Transactions`.

