using System;

namespace BNet.Cafe.Server.Services
{
    public class GridviewPaginationService
    {
        // Set PageSize to 10 so the offset increments by 10 per page
        public static readonly int PageSize = 10;

        public int PageIndex { get; set; }
        public int Offset { get; set; }

        public string SetPagination(int pageIndex)
        {
            PageIndex = pageIndex;

            // Offset calculates: 0, 10, 20, 30, 40...
            Offset = pageIndex * PageSize;

            int start = Offset;
            int end = Offset + PageSize + 1;

            return $"{start}, {end}";
        }
    }
}