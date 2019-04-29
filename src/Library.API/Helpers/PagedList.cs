using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.API.Helpers
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }

        public bool HasPrevious
        {
            get { return (CurrentPage > 1); }
        }

        public bool HasNext
        {
            get { return (CurrentPage < TotalPages); }
        }

        // constructor will not be used directly, instead static method will be called
        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int) Math.Ceiling(count / (double) pageSize);
            AddRange(items);
        }


        public static PagedList<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            // if page 2 is requested, the amount of items on page 1 will be skipped
            // then we take the current requested page size
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PagedList<T>(items,count,pageNumber,pageSize);
        }
    }
}
