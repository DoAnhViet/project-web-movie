# Tài Liệu Quản Lý Bình Luận - Admin

## Giới thiệu
Tính năng quản lý bình luận cho phép admin xem, chỉnh sửa, xóa và thống kê bình luận của người dùng trên website.

## Vị trí truy cập
- **Menu Admin**: Sidebar → Quản lý bình luận
- **Dashboard**: Click vào card "Tổng bình luận" sẽ chuyển đến trang quản lý

## Các Chức Năng Chính

### 1. Danh Sách Bình Luận (`ManageComments`)
**Đường dẫn**: `/Admin/ManageComments`

**Tính năng:**
- Hiển thị danh sách tất cả bình luận với phân trang (20 bình luận/trang)
- Lọc theo tên/slug phim
- Tìm kiếm nội dung bình luận
- Xem thông tin: người dùng, phim, nội dung, ngày tạo, lần cập nhật
- Xóa bình luận nhanh từ list
- Xem chi tiết bình luận

**Code:**
```csharp
public async Task<IActionResult> ManageComments(int page = 1, int pageSize = 20, string? filterMovie = null)
```

### 2. Chi Tiết Bình Luận (`CommentDetail`)
**Đường dẫn**: `/Admin/Comments/{id}`

**Tính năng:**
- Xem đầy đủ thông tin bình luận
- Chỉnh sửa nội dung (với kiểm tra ký tự)
- Xóa bình luận (yêu cầu xác nhận)
- Đếm ký tự tự động

**Code:**
```csharp
public async Task<IActionResult> CommentDetail(int id)
```

### 3. Chỉnh Sửa Bình Luận (AJAX)
**Endpoint**: `POST /Admin/EditComment`

**Tham số:**
- `id` (int): ID bình luận
- `content` (string): Nội dung mới (tối đa 1000 ký tự)

**Response:**
```json
{
  "success": true/false,
  "message": "Thông báo"
}
```

**Code:**
```csharp
[HttpPost]
public async Task<IActionResult> EditComment(int id, string content)
```

### 4. Xóa Bình Luận (AJAX)
**Endpoint**: `POST /Admin/DeleteComment`

**Tham số:**
- `id` (int): ID bình luận cần xóa

**Code:**
```csharp
[HttpPost]
public async Task<IActionResult> DeleteComment(int id)
```

### 5. Thống Kê Bình Luận (AJAX)
**Endpoint**: `GET /Admin/GetCommentStats`

**Response:**
```json
{
  "totalComments": 150,
  "commentsToday": 5,
  "commentsThisMonth": 42,
  "topMoviesWithComments": [
    {
      "movieSlug": "avengers",
      "movieTitle": "Avengers",
      "commentCount": 25
    }
  ],
  "topCommenters": [
    {
      "userId": "user123",
      "userName": "john_doe",
      "commentCount": 10
    }
  ]
}
```

**Tính năng:**
- Đếm tổng bình luận
- Bình luận hôm nay
- Bình luận tháng này
- Top 10 phim được bình luận nhiều nhất
- Top 10 người dùng bình luận nhiều nhất

**Code:**
```csharp
public async Task<IActionResult> GetCommentStats()
```

### 6. Xóa Tất Cả Bình Luận Của Phim (AJAX)
**Endpoint**: `POST /Admin/DeleteAllCommentsForMovie`

**Tham số:**
- `movieSlug` (string): Slug của phim

**Code:**
```csharp
[HttpPost]
public async Task<IActionResult> DeleteAllCommentsForMovie(string movieSlug)
```

### 7. Xóa Tất Cả Bình Luận Của Người Dùng (AJAX)
**Endpoint**: `POST /Admin/DeleteAllCommentsFromUser`

**Tham số:**
- `userId` (string): ID người dùng

**Code:**
```csharp
[HttpPost]
public async Task<IActionResult> DeleteAllCommentsFromUser(string userId)
```

### 8. Tìm Kiếm Bình Luận
**Đường dẫn**: `/Admin/SearchComments?query={query}`

**Tính năng:**
- Tìm kiếm theo nội dung hoặc tên phim
- Kết quả hiển thị giống trang danh sách

**Code:**
```csharp
[HttpGet]
public async Task<IActionResult> SearchComments(string query, int page = 1, int pageSize = 20)
```

## Models

### CommentManagementViewModel
```csharp
public class CommentManagementViewModel
{
    public List<MovieComment> Comments { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalComments { get; set; }
    public int PageSize { get; set; }
    public string? FilterMovie { get; set; }
    public string? SearchQuery { get; set; }
    public int CommentsToday { get; set; }
    public int CommentsThisMonth { get; set; }
    public List<MovieCommentStats>? TopMoviesWithComments { get; set; }
    public List<CommentCountByUser>? TopCommenters { get; set; }
}
```

### MovieCommentStats
```csharp
public class MovieCommentStats
{
    public string MovieSlug { get; set; }
    public string MovieTitle { get; set; }
    public int CommentCount { get; set; }
}
```

### CommentCountByUser
```csharp
public class CommentCountByUser
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public int CommentCount { get; set; }
}
```

## Views

### ManageComments.cshtml
- Hiển thị danh sách bình luận
- Bộ lọc và tìm kiếm
- Phân trang
- Nút xóa nhanh
- Thống kê tóm tắt

### CommentDetail.cshtml
- Xem chi tiết bình luận
- Chỉnh sửa nội dung
- Xóa bình luận
- Đếm ký tự

## Bảo Mật
- Tất cả actions yêu cầu `[Authorize(Roles = "Admin")]`
- Xác nhận trước khi xóa
- Validate nội dung (0-1000 ký tự)
- Xử lý exception toàn cục

## Cấu Hình Program.cs
```csharp
// CommentService đã được đăng ký:
builder.Services.AddScoped<CommentService>();
```

## Ví Dụ Sử Dụng

### JavaScript - Xóa bình luận:
```javascript
fetch('/Admin/DeleteComment', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ id: 123 })
})
.then(response => response.json())
.then(data => {
    if (data.success) {
        alert(data.message);
        location.reload();
    }
});
```

### JavaScript - Lấy thống kê:
```javascript
fetch('/Admin/GetCommentStats')
    .then(response => response.json())
    .then(data => {
        console.log('Tổng bình luận:', data.totalComments);
        console.log('Hôm nay:', data.commentsToday);
    });
```

## Công Việc Sau (Tuỳ Chọn)
1. Thêm export bình luận thành CSV
2. Thêm filter theo ngày
3. Thêm phê duyệt bình luận (moderation)
4. Thêm flag bình luận spam
5. Thêm phân tích sentiment bình luận
6. Thêm notification khi có bình luận mới

---

**Tạo bởi**: Admin System
**Ngày**: 1 tháng 12, 2025
