using Microsoft.AspNetCore.Identity;
using MebelMaster.Models;

namespace MebelMaster.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Создание ролей
            string[] roleNames = { "Admin", "Manager", "User" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Создание администратора
            var adminUser = new ApplicationUser
            {
                UserName = "admin@mebelmaster.ru",
                Email = "admin@mebelmaster.ru",
                FullName = "Администратор",
                EmailConfirmed = true
            };

            string adminPassword = "Admin123!";
            var user = await userManager.FindByEmailAsync(adminUser.Email);

            if (user == null)
            {
                var createPowerUser = await userManager.CreateAsync(adminUser, adminPassword);
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Добавление тестовых категорий
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Кухни", Description = "Кухонные гарнитуры на заказ" },
                    new Category { Name = "Шкафы-купе", Description = "Вместительные шкафы-купе" },
                    new Category { Name = "Гостиные", Description = "Мебель для гостиной" },
                    new Category { Name = "Спальни", Description = "Спальные гарнитуры" },
                    new Category { Name = "Офисная мебель", Description = "Мебель для офиса" }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // Добавление тестовых товаров
            if (!context.Products.Any())
            {
                var kitchenCategory = context.Categories.First(c => c.Name == "Кухни");

                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Кухня 'Милан'",
                        Article = "KITCH-001",
                        Description = "Современная кухня в классическом стиле из массива дуба",
                        Price = 125000m,
                        CategoryId = kitchenCategory.Id,
                        IsPopular = true,
                        Characteristics = new Dictionary<string, string>
                        {
                            { "Материал", "Массив дуба" },
                            { "Цвет", "Белый" },
                            { "Стиль", "Классический" },
                            { "Размер", "250x200x90 см" }
                        }
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }

            // Добавление тестовых отзывов
            if (!context.Reviews.Any())
            {
                var reviews = new List<Review>
                {
                    new Review
                    {
                        CustomerName = "Ирина Петрова",
                        Text = "Заказывали кухню в этой фабрике. Качество отличное, сборка профессиональная. Очень довольны результатом!",
                        Rating = 5,
                        IsApproved = true
                    },
                    new Review
                    {
                        CustomerName = "Александр Иванов",
                        Text = "Сделали шкаф-купе в спальню. Воплотили все наши пожелания, качество на высоте. Рекомендую!",
                        Rating = 5,
                        IsApproved = true
                    }
                };

                await context.Reviews.AddRangeAsync(reviews);
                await context.SaveChangesAsync();
            }
        }
    }
}