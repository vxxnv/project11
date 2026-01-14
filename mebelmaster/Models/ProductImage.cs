using System.ComponentModel.DataAnnotations;

namespace MebelMaster.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(200)]
        public string? AltText { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}