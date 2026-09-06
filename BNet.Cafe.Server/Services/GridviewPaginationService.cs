using System;

namespace BNet.Cafe.Server.Services
{
    public class GridviewPaginationService
    {
        // 1 item per page
        public static readonly int PageSize = 10;
        public int PageIndex { get; set; }
        public int Offset { get; set; }

        public string SetPagination(int pageIndex)
        {
            PageIndex = pageIndex;

            // Starting row offset for SQL (OFFSET / FETCH NEXT or SKIP / TAKE)
            Offset = pageIndex * PageSize;

            int start = Offset;
            int end = Offset + PageSize; // Exact bound limit

            return $"{start}, {PageSize}";
        }
    }
}