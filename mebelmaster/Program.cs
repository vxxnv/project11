using MebelMaster.Data;
using MebelMaster.Models;
using MebelMaster.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=MebelMaster;Trusted_Connection=true;MultipleActiveResultSets=true";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Настройка Identity с кастомными путями
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddErrorDescriber<RussianIdentityErrorDescriber>();

// Настройка кастомных путей для Identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ReturnUrlParameter = "returnUrl";
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Регистрируем кастомные сервисы
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

var app = builder.Build();

// Инициализация БД
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.EnsureCreatedAsync();
        await DbInitializer.InitializeAsync(context, userManager, roleManager);

        Console.WriteLine("MebelMaster database created successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating MebelMaster database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Маршруты для Account
app.MapControllerRoute(
    name: "account",
    pattern: "Account/{action=Login}/{id?}",
    defaults: new { controller = "Account" });

// Специфичные маршруты для админ-зоны
app.MapAreaControllerRoute(
    name: "AdminArea",
    areaName: "Admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

// Общие маршруты для обычных контроллеров
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

// Класс для русских сообщений об ошибках Identity
public class RussianIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() 
        => new IdentityError { Code = nameof(DefaultError), Description = "Произошла неизвестная ошибка." };

    public override IdentityError PasswordRequiresDigit() 
        => new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "Пароль должен содержать цифры (0-9)." };

    public override IdentityError PasswordRequiresLower() 
        => new IdentityError { Code = nameof(PasswordRequiresLower), Description = "Пароль должен содержать строчные буквы (a-z)." };

    public override IdentityError PasswordRequiresUpper() 
        => new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "Пароль должен содержать заглавные буквы (A-Z)." };

    public override IdentityError PasswordRequiresNonAlphanumeric() 
        => new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Пароль должен содержать хотя бы один символ, не являющийся буквой или цифрой." };

    public override IdentityError PasswordTooShort(int length) 
        => new IdentityError { Code = nameof(PasswordTooShort), Description = $"Пароль должен быть не менее {length} символов." };

    public override IdentityError DuplicateUserName(string userName) 
        => new IdentityError { Code = nameof(DuplicateUserName), Description = $"Пользователь с именем '{userName}' уже существует." };

    public override IdentityError DuplicateEmail(string email) 
        => new IdentityError { Code = nameof(DuplicateEmail), Description = $"Email '{email}' уже используется." };

    public override IdentityError InvalidUserName(string userName) 
        => new IdentityError { Code = nameof(InvalidUserName), Description = "Недопустимое имя пользователя." };

    public override IdentityError InvalidEmail(string email) 
        => new IdentityError { Code = nameof(InvalidEmail), Description = "Недопустимый email адрес." };

    public override IdentityError LoginAlreadyAssociated() 
        => new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = "Пользователь с таким логином уже существует." };

    public override IdentityError InvalidToken() 
        => new IdentityError { Code = nameof(InvalidToken), Description = "Неверный токен." };

    public override IdentityError PasswordMismatch() 
        => new IdentityError { Code = nameof(PasswordMismatch), Description = "Неверный пароль." };

    public override IdentityError UserAlreadyInRole(string role) 
        => new IdentityError { Code = nameof(UserAlreadyInRole), Description = $"Пользователь уже имеет роль '{role}'." };

    public override IdentityError UserNotInRole(string role) 
        => new IdentityError { Code = nameof(UserNotInRole), Description = $"Пользователь не имеет роли '{role}'." };
}