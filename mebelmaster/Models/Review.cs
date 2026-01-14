using System.ComponentModel.DataAnnotations;

namespace MebelMaster.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(100, ErrorMessage = "Имя не может превышать 100 символов")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Текст отзыва обязателен")]
        [StringLength(1000, ErrorMessage = "Отзыв не может превышать 1000 символов")]
        public string Text { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Рейтинг должен быть от 1 до 5")]
        public int Rating { get; set; } = 5;

        public bool IsApproved { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}