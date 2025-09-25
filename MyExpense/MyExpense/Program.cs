using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using MyExpense.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Index");
});

// EF Core DbContext
builder.Services.AddDbContext<ExpenseManagerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBDefault")));

// Authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = builder.Configuration["Authentication:Cookie:LoginPath"] ?? "/account/login";
        options.LogoutPath = builder.Configuration["Authentication:Cookie:LogoutPath"] ?? "/account/logout";
        options.AccessDeniedPath = builder.Configuration["Authentication:Cookie:AccessDeniedPath"] ?? "/account/access-denied";
        options.SlidingExpiration = true;
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.SaveTokens = true;
        options.Events.OnCreatingTicket = async context =>
        {
            var googleId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var name = context.Principal?.FindFirst(ClaimTypes.Name)?.Value;
            var picture = context.User.TryGetProperty("picture", out var pic) ? pic.GetString() : null;

            using var scope = context.HttpContext.RequestServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ExpenseManagerContext>();

            var user = await db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId || u.Email == email);
            if (user == null)
            {
                user = new User
                {
                    GoogleId = googleId ?? Guid.NewGuid().ToString(),
                    Email = email,
                    Username = email,
                    FullName = name,
                    PictureUrl = picture,
                    Role = "USER",
                    Enabled = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow
                };
                db.Users.Add(user);
            }
            else
            {
                user.GoogleId = googleId ?? user.GoogleId;
                user.FullName = name;
                user.PictureUrl = picture;
                user.UpdatedAt = DateTime.UtcNow;
                user.LastLogin = DateTime.UtcNow;
            }
            await db.SaveChangesAsync();

            var identity = (ClaimsIdentity)context.Principal!.Identity!;
            identity.AddClaim(new Claim(ClaimTypes.Role, user.Role));
            identity.AddClaim(new Claim("app:userId", user.Id.ToString()));
        };
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("ADMIN"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Auth endpoints
app.MapGet("/account/login", () => Results.Redirect("/Account/Login")).AllowAnonymous();

app.MapGet("/account/google", async context =>
{
    await context.ChallengeAsync(
        GoogleDefaults.AuthenticationScheme,
        new AuthenticationProperties { RedirectUri = "/Account/Profile" }
    );
}).AllowAnonymous();

app.MapPost("/account/signout", async context =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" });
});

app.MapGet("/account/logout", () => Results.Redirect("/Account/Logout"));

app.Run();
