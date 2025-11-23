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
builder.Services.AddScoped<IBaseStorage<CategoryDb>, CategoryStorage>(); // Новое
builder.Services.AddScoped<IBaseStorage<CourseDb>, CourseStorage>();     // Новое

// Подключение AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Подключение Сервиса Аккаунта
builder.Services.AddScoped<IAccountService, AccountService>();
// Добавляем аутентификацию с использованием куки
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Home/Login");
        options.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/Home/Index");
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Разрешить куки без HTTPS
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