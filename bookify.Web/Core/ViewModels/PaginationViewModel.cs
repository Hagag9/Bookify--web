﻿namespace bookify.Web.Core.ViewModels
{
    public class PaginationViewModel
    {
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;

        public int Start
        {
            get
            {
                var start = 1;

                if (TotalPages > (int)ReportsConfigurations.MaxPaginationNumber)
                    start = PageNumber - 9 < 1 ? 1 : PageNumber - 9;

                return start;
            }
        }
        public int End
        {
            get
            {
                var end = TotalPages;

                if (TotalPages > (int)ReportsConfigurations.MaxPaginationNumber)
                    end = Start + (int)ReportsConfigurations.MaxPaginationNumber - 1 > TotalPages ? TotalPages : Start + (int)ReportsConfigurations.MaxPaginationNumber - 1;

                return end;
            }
        }
    }
}
