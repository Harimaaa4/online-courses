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

builder.Services.AddScoped<IBaseStorage<UserDb>, UserStorage>();
builder.Services.AddScoped<IBaseStorage<CategoryDb>, CategoryStorage>();
builder.Services.AddScoped<IBaseStorage<CourseDb>, CourseStorage>();

// Подключение AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Подключение Сервиса Аккаунта
builder.Services.AddScoped<IAccountService, AccountService>();

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
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Разрешить куки без HTTPS
})
.AddGoogle(options =>
{
    // Берем ключи из конфигурации (secrets.json или appsettings.json)
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
});

var app = builder.Build();

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

app.UseAuthentication(); // Сначала проверяем, кто это
app.UseAuthorization();  // Потом проверяем, можно ли ему сюда

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "siteinfo",
    pattern: "SiteInformation",
    defaults: new { controller = "Home", action = "SiteInformation" });

app.Run();