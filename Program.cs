// Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// === —≈–¬»—€ (ËÁ Startup.ConfigureServices) ===
builder.Services.AddControllersWithViews();

var app = builder.Build();

// === œ¿…œÀ¿…Õ (ËÁ Startup.Configure) ===
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ”ƒ¿À≈ÕŒ: app.UseHttpsRedirection(); ó HTTPS Õ≈ Õ”∆≈Õ!

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "siteinfo",
    pattern: "SiteInformation",
    defaults: new { controller = "Home", action = "SiteInformation" });

app.Run();