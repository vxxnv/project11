using System.ComponentModel.DataAnnotations;

namespace MebelMaster.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(100, ErrorMessage = "Имя не может превышать 100 символов")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Телефон обязателен")]
        [Phone(ErrorMessage = "Неверный формат телефона")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Неверный формат email")]
        public string? Email { get; set; }

        [StringLength(1000, ErrorMessage = "Сообщение не может превышать 1000 символов")]
        public string? Message { get; set; }

        // Связь с товаром (если заявка привязана к конкретному товару)
        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.New;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum OrderStatus
    {
        New,
        InProgress,
        Completed,
        Cancelled
    }
}