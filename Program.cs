using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; 
using online_courses.Data;

var builder = WebApplication.CreateBuilder(args);

// === —≈–¬»—€ ===
builder.Services.AddControllersWithViews();

// œÓ‰ÍÎ˛˜ÂÌËÂ Í PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// === œ¿…œÀ¿…Õ ===
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
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "siteinfo",
    pattern: "SiteInformation",
    defaults: new { controller = "Home", action = "SiteInformation" });

app.Run();