using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<MoneyTrackerApp.Models.ExpenseManagerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBDefault"))
);
builder.Services.AddScoped<MoneyTrackerApp.Services.JwtTokenService>();

// Register Wallet Management Services
builder.Services.AddScoped<MoneyTrackerApp.Services.IAccountService, MoneyTrackerApp.Services.AccountService>();
builder.Services.AddScoped<MoneyTrackerApp.Services.ISharedAccountService, MoneyTrackerApp.Services.SharedAccountService>();
builder.Services.AddScoped<MoneyTrackerApp.Services.IBankConnectionService, MoneyTrackerApp.Services.BankConnectionService>();
builder.Services.AddScoped<MoneyTrackerApp.Services.INetWorthService, MoneyTrackerApp.Services.NetWorthService>();

// Register Transaction Management Services
builder.Services.AddScoped<MoneyTrackerApp.Services.ITransactionService, MoneyTrackerApp.Services.TransactionService>();
builder.Services.AddScoped<MoneyTrackerApp.Services.ICategoryService, MoneyTrackerApp.Services.CategoryService>();
builder.Services.AddScoped<MoneyTrackerApp.Services.IScheduledTransactionService, MoneyTrackerApp.Services.ScheduledTransactionService>();
builder.Services.AddScoped<MoneyTrackerApp.Services.IOcrService, MoneyTrackerApp.Services.OcrService>();

// Register Planning Services
builder.Services.AddScoped<MoneyTrackerApp.Services.IBudgetService, MoneyTrackerApp.Services.BudgetService>();
builder.Services.AddScoped<MoneyTrackerApp.Services.ISavingsGoalService, MoneyTrackerApp.Services.SavingsGoalService>();

// Register Debt & Investment Services
builder.Services.AddScoped<MoneyTrackerApp.Services.IDebtService, MoneyTrackerApp.Services.DebtService>();
builder.Services.AddScoped<MoneyTrackerApp.Services.IInvestmentService, MoneyTrackerApp.Services.InvestmentService>();

// Register Group & Split Bill Services
builder.Services.AddScoped<MoneyTrackerApp.Services.IGroupExpenseService, MoneyTrackerApp.Services.GroupExpenseService>();

// Register Report & Analytics Services
builder.Services.AddScoped<MoneyTrackerApp.Services.IReportService, MoneyTrackerApp.Services.ReportService>();
builder.Services.AddScoped<MoneyTrackerApp.Services.IExportService, MoneyTrackerApp.Services.ExportService>();

// Register System Utility Services
builder.Services.AddScoped<MoneyTrackerApp.Services.INotificationService, MoneyTrackerApp.Services.NotificationService>();
builder.Services.AddScoped<MoneyTrackerApp.Services.ICurrencyService, MoneyTrackerApp.Services.CurrencyService>();
builder.Services.AddScoped<MoneyTrackerApp.Services.IAiAdvisorService, MoneyTrackerApp.Services.AiAdvisorService>();

// Register Onboarding Service
// Register Onboarding Service
builder.Services.AddScoped<MoneyTrackerApp.Services.OnboardingService>();

// Register Admin Services
builder.Services.AddScoped<MoneyTrackerApp.Services.IUserManagementService, MoneyTrackerApp.Services.UserManagementService>();
builder.Services.AddScoped<MoneyTrackerApp.Services.ISystemSettingsService, MoneyTrackerApp.Services.SystemSettingsService>();
builder.Services.AddScoped<MoneyTrackerApp.Services.IAdminDashboardService, MoneyTrackerApp.Services.AdminDashboardService>();

// Register Subscription Service
builder.Services.AddScoped<MoneyTrackerApp.Services.ISubscriptionService, MoneyTrackerApp.Services.SubscriptionService>();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtIssuer = jwtSection.GetValue<string>("Issuer");
var jwtAudience = jwtSection.GetValue<string>("Audience");
var jwtKey = jwtSection.GetValue<string>("Key");

var auth = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
});

auth.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey ?? "dev-secret-key")),
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("AccessToken"))
            {
                context.Token = context.Request.Cookies["AccessToken"];
            }
            return Task.CompletedTask;
        }
    };
});

auth.AddCookie("External");

var googleSection = builder.Configuration.GetSection("Authentication:Google");
auth.AddGoogle(options =>
{
    options.ClientId = googleSection.GetValue<string>("ClientId") ?? "";
    options.ClientSecret = googleSection.GetValue<string>("ClientSecret") ?? "";
    options.SignInScheme = "External";
    options.CallbackPath = "/signin-google";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
var supportedCultures = new[] { new System.Globalization.CultureInfo("vi-VN") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("vi-VN"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

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

app.UseMiddleware<MoneyTrackerApp.Middleware.OnboardingMiddleware>();

app.MapRazorPages();
app.MapControllers();

app.Run();
