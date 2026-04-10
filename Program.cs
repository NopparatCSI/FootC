using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WebApp1.Models.Db;
using WebApp1.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.AccessDeniedPath = "/Admin/AccessDenied";
        options.Cookie.Name = "WebApp1.AdminAuth";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AdminPolicies.AccessAdminArea, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(AdminRoles.SuperAdmin, AdminRoles.Admin));

    options.AddPolicy(AdminPolicies.ManageAdmins, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(AdminRoles.SuperAdmin));

    options.AddPolicy(AdminPolicies.ManageCatalog, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(AdminRoles.SuperAdmin, AdminRoles.Admin));

    options.AddPolicy(AdminPolicies.ManageOrders, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(AdminRoles.SuperAdmin, AdminRoles.Packer));

    options.AddPolicy(AdminPolicies.ManageUsers, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(AdminRoles.SuperAdmin, AdminRoles.Admin));
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddDbContext<Csi402dbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
