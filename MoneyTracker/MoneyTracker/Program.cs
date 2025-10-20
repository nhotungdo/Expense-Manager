using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// DbContext
builder.Services.AddDbContext<MoneyTracker.Models.ExpenseManagerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBDefault")));

// Auth: cookies
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
})
#if !DEBUG
// prevent accidental external callback redirect loops in non-debug builds
#endif
// External providers (commented out - requires Microsoft.AspNetCore.Authentication.Google/Facebook packages)
// To enable Google/Facebook authentication, add the corresponding NuGet packages and then uncomment these.
// .AddGoogle(...).AddFacebook(...);
;

// Password hasher
builder.Services.AddScoped<IPasswordHasher<MoneyTracker.Models.User>, PasswordHasher<MoneyTracker.Models.User>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
