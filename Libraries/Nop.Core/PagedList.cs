using System.Collections.Generic;
using System.Linq;

namespace Nop.Core
{
    /// <summary>
    /// Paged list
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    public class PagedList<T> : List<T>, IPagedList<T> 
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        public PagedList(IQueryable<T> source, int pageIndex, int pageSize)
        {
			this.PageSize = pageSize;
			this.TotalCount = source.Count();
            this.PageIndex = pageIndex;
            this.AddRange(source.Skip(pageIndex * pageSize).Take(pageSize).ToList());
        }

		public PagedList(IEnumerable<T> source, int pageIndex, int pageSize)
		{
			this.PageSize = pageSize;
			this.TotalCount = source.Count();
			this.PageIndex = pageIndex;
			this.AddRange(source.Skip(pageIndex * pageSize).Take(pageSize).ToList());
		}

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        public PagedList(IList<T> source, int pageIndex, int pageSize)
        {
			this.PageSize = pageSize;
			TotalCount = source.Count();
            this.PageIndex = pageIndex;
            this.AddRange(source.Skip(pageIndex * pageSize).Take(pageSize).ToList());
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalCount">Total count</param>
        public PagedList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
        {
			this.PageSize = pageSize;
            TotalCount = totalCount;
            this.PageIndex = pageIndex;
            this.AddRange(source);
        }

        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }

		int _totalCount;
        public int TotalCount { 
			get { return _totalCount; }
			set {
				_totalCount = value;
				if (PageSize < 1) PageSize = int.MaxValue;
				TotalPages = _totalCount / PageSize;

				if (_totalCount % PageSize > 0)
					TotalPages++;
			}
		}
        public int TotalPages { get; private set; }

        public bool HasPreviousPage
        {
            get { return (PageIndex > 0); }
        }
        public bool HasNextPage
        {
            get { return (PageIndex + 1 < TotalPages); }
        }
    }
}
