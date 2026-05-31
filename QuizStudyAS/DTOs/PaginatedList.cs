using System;
using System.Collections.Generic;
using System.Linq;

namespace QuizStudyAS.DTOs
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; set; }
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalCount = count;
            Items = items;
        }

        // 2 thuộc tính giúp UI biết khi nào thì ẩn/hiện nút "Trang trước", "Trang sau"
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        // Hàm tiện ích (Factory Method) để tự động tính toán Skip/Take từ IQueryable
        public static PaginatedList<T> Create(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count(); // Đếm TỔNG SỐ bản ghi thỏa mãn điều kiện lọc

            // Lệnh phân trang cốt lõi của Entity Framework Core nằm ở đây:
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}