using System.Collections.Generic;

namespace WebMovie.Helpers
{
    public static class CategoryNames
    {
        private static readonly Dictionary<string, string> Map = new Dictionary<string, string>
        {
            // Thể loại phổ biến
            { "hanh-dong", "Hành Động" },
            { "phim-hanh-dong", "Hành Động" },
            { "hanh-dong-han", "Hành Động Hàn" },
            { "tinh-cam", "Tình Cảm" },
            { "phim-tinh-cam", "Tình Cảm" },
            { "hai-huoc", "Hài Hước" },
            { "phim-hai-huoc", "Hài Hước" },
            { "kinh-di", "Kinh Dị" },
            { "phim-kinh-di", "Kinh Dị" },
            { "vo-thuat", "Võ Thuật" },
            { "phieu-luu", "Phiêu Lưu" },
            { "vien-tuong", "Viễn Tưởng" },
            { "phim-vien-tuong", "Viễn Tưởng" },
            { "tam-ly", "Tâm Lý" },
            { "chinh-kich", "Chính Kịch" },
            { "hinh-su", "Hình Sự" },
            { "trinh-tham", "Trinh Thám" },
            { "hoat-hinh", "Hoạt Hình" },
            { "phim-hoat-hinh", "Hoạt Hình" },
            { "anime", "Anime" },
            { "phim-chieu-rap", "Chiếu Rạp" },
            { "chieu-rap", "Chiếu Rạp" },
            { "phim-le", "Phim Lẻ" },
            { "le", "Phim Lẻ" },
            { "phim-bo", "Phim Bộ" },
            { "bo", "Phim Bộ" },
            { "tai-lieu", "Tài Liệu" },
            { "lich-su", "Lịch Sử" },
            { "am-nhac", "Âm Nhạc" },
            { "thieu-nhi", "Thiếu Nhi" },
            { "gia-dinh", "Gia Đình" },
            { "the-thao", "Thể Thao" },
            { "khoa-hoc", "Khoa Học" },
            { "hai-huoc-hanh-dong", "Hài Hước - Hành Động" },
            { "phim-kiem-hiep", "Kiếm Hiệp" },
            { "kiem-hiep", "Kiếm Hiệp" },
            { "phim-tam-ly", "Tâm Lý" },
            { "phim-luong-tam", "Lưỡng Tâm" },
            { "phim-kinh-dinh", "Kinh Dị" },
            // Countries occasionally appear in category lists
            { "han-quoc", "Hàn Quốc" },
            { "trung-quoc", "Trung Quốc" },
            { "nhat-ban", "Nhật Bản" },
            { "my", "Mỹ" },
            { "viet-nam", "Việt Nam" },
            { "hong-kong", "Hồng Kông" },
            { "thai-lan", "Thái Lan" },
            { "dai-loan", "Đài Loan" },
            { "an-do", "Ấn Độ" }
        };

        public static string GetDisplayName(string? slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return string.Empty;
            var key = slug.Trim().ToLowerInvariant();
            if (Map.ContainsKey(key)) return Map[key];
            // fallback: replace dashes with spaces and capitalize first letter
            var fallback = key.Replace("-", " ");
            if (string.IsNullOrEmpty(fallback)) return string.Empty;
            return char.ToUpper(fallback[0]) + (fallback.Length > 1 ? fallback.Substring(1) : string.Empty);
        }
    }
}
