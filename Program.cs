using System;
using BookApp.Data;
using BookApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BookLyticsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("BookLytics")));
builder.Services.AddScoped<IBookRepository, EfBookRepository>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddHttpClient<IGoogleBooksService, GoogleBooksService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["GoogleBooks:ApiBaseUrl"]!);
});
builder.Services.AddScoped<IUserService, EfUserService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opts => {
        opts.LoginPath = "/Account/Login";
        opts.LogoutPath = "/Account/Logout";
        opts.ExpireTimeSpan = TimeSpan.FromHours(1);
    });
builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Apply migrations and seed demo data on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookLyticsDbContext>();
    DbSeeder.Initialize(db, app.Environment.ContentRootPath);
}

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();

// auth
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();