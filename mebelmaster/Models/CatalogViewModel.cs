namespace MebelMaster.Models
{
    public class CatalogViewModel
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public int? SelectedCategoryId { get; set; }
        public PageViewModel PageViewModel { get; set; } = new PageViewModel(0, 1, 9);
        public string SortOrder { get; set; } = "name";
        public string SearchString { get; set; } = string.Empty;
    }

    public class PageViewModel
    {
        public PageViewModel(int count, int page, int pageSize)
        {
            CurrentPage = page;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            PageSize = pageSize;
            TotalCount = count;
        }

        public int CurrentPage { get; }
        public int TotalPages { get; }
        public int PageSize { get; }
        public int TotalCount { get; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}