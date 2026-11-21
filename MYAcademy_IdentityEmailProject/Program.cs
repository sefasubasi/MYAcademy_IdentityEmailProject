
using Microsoft.AspNetCore.Identity;
using MYAcademy_IdentityEmailProject.Context;
using MYAcademy_IdentityEmailProject.Entities;

var builder = WebApplication.CreateBuilder(args);

// SQL Server - Entity Framework - Identity
builder.Services.AddDbContext<EmailContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login/SignIn";
    options.AccessDeniedPath = "/ErrorPages/Page403";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<EmailContext>()
    .AddDefaultTokenProviders();

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// HTTP Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
