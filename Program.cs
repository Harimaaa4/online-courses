using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using online_courses.Data;
using online_courses.Entities;
using online_courses.Interfaces;
using online_courses.Repositories;
using online_courses.Services.Implementations;
using online_courses.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// === СЕРВИСЫ ===
builder.Services.AddControllersWithViews();

// Подключение к PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Регистрация Хранилищ (Repositories)
builder.Services.AddScoped<IBaseStorage<UserDb>, UserStorage>();
builder.Services.AddScoped<IBaseStorage<CategoryDb>, CategoryStorage>();
builder.Services.AddScoped<IBaseStorage<CourseDb>, CourseStorage>();
builder.Services.AddScoped<IBaseStorage<CartDb>, CartStorage>();

// Подключение AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Регистрация Сервисов (Services)
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IInitializer, Initializer>();

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICartService, CartService>();

// === АУТЕНТИФИКАЦИЯ (Cookie + Google) ===
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Home/Login");
    options.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/Home/Index");
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
});

var app = builder.Build();

// !!! И ВОТ ЭТОГО БЛОКА НЕ ХВАТАЛО (ОН ЗАПОЛНЯЕТ БАЗУ) !!!
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var initializer = services.GetRequiredService<IInitializer>();
    await initializer.Initialize();
}
// ========================================================

// === ПАЙПЛАЙН ===
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "siteinfo",
    pattern: "SiteInformation",
    defaults: new { controller = "Home", action = "SiteInformation" });

app.Run();