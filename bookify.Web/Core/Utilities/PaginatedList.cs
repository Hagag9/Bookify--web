﻿namespace bookify.Web.Core.Utilities
{
    public class PaginatedList<T> : List<T>
    {
        public int PageNumber { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;

        public static PaginatedList<T> Create(IQueryable<T> source, int PageNumber, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((PageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, PageNumber, pageSize);
        }
    }
}
