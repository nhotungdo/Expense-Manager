using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MoneyTracker.Models;
using MoneyTracker.Services;
using MoneyTracker.Middleware;
using Serilog;
using MoneyTracker.Migrations;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Add Entity Framework
builder.Services.AddDbContext<ExpenseManagerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Authentication - Minimal configuration
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.Cookie.Name = "MoneyTrackerAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IDefaultCategoryService, DefaultCategoryService>();
builder.Services.AddScoped<IReportExportService, ReportExportService>();
builder.Services.AddScoped<IAdvancedSearchService, AdvancedSearchService>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<IAdvancedAnalyticsService, AdvancedAnalyticsService>();
builder.Services.AddScoped<DefaultAdminService>();

// Add new services
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IIncomeService, IncomeService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAISuggestionService, AISuggestionService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IScheduledEmailService, ScheduledEmailService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IPerformanceService, PerformanceService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();

// Add background services
builder.Services.AddHostedService<EmailBackgroundService>();

// Add caching
builder.Services.AddMemoryCache();

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();




// Google Auth configuration is handled in JWT configuration above

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("AllowAll");
app.UseRouting();

// Add authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Temporarily disable custom middleware to isolate the issue
// app.UseMiddleware<GlobalExceptionMiddleware>();
// app.UseMiddleware<AuditMiddleware>();

app.MapRazorPages();
app.MapControllers();

// Default route to HomePage
app.MapGet("/", () => Results.Redirect("/HomePage"));

// Ensure database is created and run migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ExpenseManagerContext>();
    context.Database.EnsureCreated();

    // Apply migrations to ensure default admin user is created
    try
    {
        context.Database.Migrate();
        Log.Information("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error applying database migrations");
    }

    // Ensure default admin user exists
    try
    {
        var defaultAdminService = scope.ServiceProvider.GetRequiredService<DefaultAdminService>();
        await defaultAdminService.EnsureDefaultAdminExistsAsync();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error ensuring default admin user exists");
    }
}

app.Run();
