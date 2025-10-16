using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;
using MoneyTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToFolder("/Home");
    options.Conventions.AllowAnonymousToFolder("/Auth");
    options.Conventions.AllowAnonymousToPage("/MoneyTracker/Onboarding");
});

// DbContext
builder.Services.AddDbContext<ExpenseManagerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBDefault")));

builder.Services.AddHttpClient();

// App services
builder.Services.AddScoped<IAiService, OpenAiService>();
builder.Services.AddScoped<IEmailService, MailKitEmailService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddHostedService<DailyReminderService>();
builder.Services.AddHostedService<RecurringTransactionService>();

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/";
})
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["GoogleAuth:ClientId"] ?? string.Empty;
    googleOptions.ClientSecret = builder.Configuration["GoogleAuth:ClientSecret"] ?? string.Empty;
    googleOptions.CallbackPath = builder.Configuration["GoogleAuth:CallbackPath"] ?? "/signin-google";
    googleOptions.Events = new OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            var db = context.HttpContext.RequestServices.GetRequiredService<ExpenseManagerContext>();
            var googleId = context.Identity?.FindFirst("sub")?.Value
                            ?? context.Identity?.FindFirst("urn:google:userid")?.Value
                            ?? context.Identity?.FindFirst("https://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            var email = context.Identity?.FindFirst("email")?.Value
                        ?? context.Identity?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
            var name = context.Identity?.FindFirst("name")?.Value
                       ?? context.Identity?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;

            if (!string.IsNullOrWhiteSpace(googleId) && !string.IsNullOrWhiteSpace(email))
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
                if (user == null)
                {
                    user = new User
                    {
                        GoogleId = googleId,
                        Email = email,
                        UserName = email,
                        FullName = name,
                        Enabled = true,
                        CreatedAt = DateTime.UtcNow,
                        Language = "vi",
                        DefaultCurrency = "VND",
                        Timezone = "Asia/Ho_Chi_Minh",
                        Theme = "light",
                        EmailNotifications = true,
                        PushNotifications = true,
                        Role = "User",
                        OnboardingCompleted = false
                    };
                    db.Users.Add(user);
                }
                else
                {
                    user.Email = email;
                    user.FullName = name;
                    user.LastLogin = DateTime.UtcNow;
                }
                await db.SaveChangesAsync();
            }
        }
    };
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

// --- AI suggestions endpoint ---
app.MapPost("/api/ai/suggestions", async (ExpenseManagerContext db, IAiService ai) =>
{
    var last30 = await db.Transactions
        .OrderByDescending(t => t.TransactionDate)
        .Take(30)
        .Select(t => new AiTransactionInput
        {
            Date = t.TransactionDate,
            CategoryId = t.CategoryId,
            Amount = t.Amount
        })
        .ToListAsync();

    var tips = await ai.GetSuggestionsAsync(last30);
    return Results.Ok(new { suggestions = tips });
});

// --- Onboarding completion endpoint ---
app.MapPost("/api/onboarding/complete", async (ExpenseManagerContext db, HttpContext ctx, dynamic body) =>
{
    var googleId = ctx.User?.FindFirst("sub")?.Value
        ?? ctx.User?.FindFirst("urn:google:userid")?.Value;
    if (string.IsNullOrWhiteSpace(googleId)) return Results.Unauthorized();

    var user = await db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
    if (user == null) return Results.NotFound();

    // Read optional fields (not persisted yet except onboarding flag)
    string defaultWallet = body?.defaultWallet;
    decimal? savingGoal = body?.savingGoal;
    bool? enableAi = body?.enableAi;

    user.OnboardingCompleted = true;
    user.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapGet("/login", async context =>
{
    await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = "/MoneyTracker/Onboarding"
    });
}).AllowAnonymous();

app.MapGet("/logout", async context =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = "/"
    });
});

// Redirect root path to Home
app.MapGet("/", context =>
{
    context.Response.Redirect("/Home");
    return Task.CompletedTask;
}).AllowAnonymous();

// Redirect legacy/top-level Pages routes to Home
app.MapGet("/Dashboard/{*path}", context =>
{
    context.Response.Redirect("/Home");
    return Task.CompletedTask;
}).AllowAnonymous();

// Redirect old FinTrack area to new MoneyTracker area
app.MapGet("/FinTrack/{*path}", context =>
{
    var path = context.Request.Path.Value?.Substring("/FinTrack".Length) ?? string.Empty;
    context.Response.Redirect($"/MoneyTracker{path}");
    return Task.CompletedTask;
}).AllowAnonymous();

// Map MoneyTracker area (no redirect to Home)
app.MapRazorPages();

app.Run();

public partial class Program { }
