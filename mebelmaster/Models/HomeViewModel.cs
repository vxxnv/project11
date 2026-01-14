namespace MebelMaster.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Product> PopularProducts { get; set; } = new List<Product>();
        public IEnumerable<Review> Reviews { get; set; } = new List<Review>();
    }
}