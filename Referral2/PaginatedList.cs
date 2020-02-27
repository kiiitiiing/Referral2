using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2
{
    public class PaginatedList<T> : List<T>
    {
        public int _pageIndex { get; private set; }
        public int _totalPages { get; private set; }
        public int _pageSize { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            _pageIndex = pageIndex;
            _totalPages = (int)Math.Ceiling(count / (double)pageSize);
            _pageSize = pageSize;
            this.AddRange(items);
        }

        public bool HasPreviousPage { get { return (_pageIndex > 1); } }

        public bool HasNextPage { get { return (_pageIndex < _totalPages); } }

        public static async Task<PaginatedList<T>> CreateAsync(IOrderedQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}
