using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace MebelMaster.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название товара обязательно")]
        [StringLength(200, ErrorMessage = "Название не может превышать 200 символов")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Артикул обязателен")]
        [StringLength(50, ErrorMessage = "Артикул не может превышать 50 символов")]
        public string Article { get; set; } = string.Empty;

        [Required(ErrorMessage = "Описание товара обязательно")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Цена обязательна")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть больше 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string? CharacteristicsJson { get; set; }

        [NotMapped]
        public Dictionary<string, string> Characteristics
        {
            get
            {
                if (string.IsNullOrEmpty(CharacteristicsJson))
                    return new Dictionary<string, string>();

                return JsonSerializer.Deserialize<Dictionary<string, string>>(CharacteristicsJson)
                    ?? new Dictionary<string, string>();
            }
            set
            {
                CharacteristicsJson = JsonSerializer.Serialize(value);
            }
        }

        // ВРЕМЕННО: уберите Required и сделайте Category nullable
        public int CategoryId { get; set; }
        
        public Category? Category { get; set; } // Сделайте nullable

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

        public bool IsPopular { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}