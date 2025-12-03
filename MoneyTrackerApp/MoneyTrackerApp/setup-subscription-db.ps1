# Subscription Database Setup Script
# This script checks and sets up the subscription tables and seed data

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Subscription System Database Setup" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Get connection string from appsettings.json
$appsettingsPath = Join-Path $PSScriptRoot "appsettings.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    $connectionString = $appsettings.ConnectionStrings.DefaultConnection
    Write-Host "✓ Found connection string in appsettings.json" -ForegroundColor Green
} else {
    Write-Host "✗ appsettings.json not found" -ForegroundColor Red
    Write-Host "Using default connection string from ExpenseManagerContext.cs" -ForegroundColor Yellow
    $connectionString = "Data Source=NHOTUNG\SQLEXPRESS;Database=ExpenseManager;User Id=sa;Password=123;TrustServerCertificate=true;Trusted_Connection=SSPI;Encrypt=false;"
}

Write-Host ""
Write-Host "Connection String: $connectionString" -ForegroundColor Gray
Write-Host ""

# Path to migration script
$migrationScript = Join-Path $PSScriptRoot "Migrations\AddSubscriptionTables.sql"

if (-not (Test-Path $migrationScript)) {
    Write-Host "✗ Migration script not found: $migrationScript" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Found migration script" -ForegroundColor Green
Write-Host ""

# Ask for confirmation
Write-Host "This script will:" -ForegroundColor Yellow
Write-Host "  1. Create ServicePackages, Subscriptions, and Payments tables (if not exist)" -ForegroundColor Yellow
Write-Host "  2. Create necessary triggers" -ForegroundColor Yellow
Write-Host "  3. Insert default service packages (Free, Pro, Team)" -ForegroundColor Yellow
Write-Host ""

$confirmation = Read-Host "Do you want to proceed? (Y/N)"
if ($confirmation -ne 'Y' -and $confirmation -ne 'y') {
    Write-Host "Operation cancelled." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Executing migration script..." -ForegroundColor Cyan

try {
    # Execute SQL script using sqlcmd
    $server = if ($connectionString -match "Data Source=([^;]+)") { $matches[1] } else { "localhost" }
    $database = if ($connectionString -match "Database=([^;]+)") { $matches[1] } else { "ExpenseManager" }
    
    Write-Host "Server: $server" -ForegroundColor Gray
    Write-Host "Database: $database" -ForegroundColor Gray
    Write-Host ""
    
    # Check if sqlcmd is available
    $sqlcmdPath = Get-Command sqlcmd -ErrorAction SilentlyContinue
    
    if ($null -eq $sqlcmdPath) {
        Write-Host "✗ sqlcmd not found in PATH" -ForegroundColor Red
        Write-Host ""
        Write-Host "Please install SQL Server Command Line Utilities or run the script manually:" -ForegroundColor Yellow
        Write-Host "  sqlcmd -S $server -d $database -i `"$migrationScript`"" -ForegroundColor White
        Write-Host ""
        Write-Host "Alternative: Open SQL Server Management Studio and execute the script manually." -ForegroundColor Yellow
        exit 1
    }
    
    # Execute the migration
    Write-Host "Running migration..." -ForegroundColor Cyan
    $output = sqlcmd -S $server -d $database -i $migrationScript -E 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "==================================================" -ForegroundColor Green
        Write-Host "✓ Migration completed successfully!" -ForegroundColor Green
        Write-Host "==================================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "Output:" -ForegroundColor Gray
        Write-Host $output -ForegroundColor Gray
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor Cyan
        Write-Host "  1. Run the application: dotnet run" -ForegroundColor White
        Write-Host "  2. Navigate to: http://localhost:5000/Subscription" -ForegroundColor White
        Write-Host "  3. You should see 3 service packages (Free, Pro, Team)" -ForegroundColor White
    } else {
        Write-Host ""
        Write-Host "✗ Migration failed with exit code: $LASTEXITCODE" -ForegroundColor Red
        Write-Host ""
        Write-Host "Error output:" -ForegroundColor Red
        Write-Host $output -ForegroundColor Red
        Write-Host ""
        Write-Host "Please check:" -ForegroundColor Yellow
        Write-Host "  1. SQL Server is running" -ForegroundColor White
        Write-Host "  2. Database 'ExpenseManager' exists" -ForegroundColor White
        Write-Host "  3. You have permissions to create tables" -ForegroundColor White
        exit 1
    }
} catch {
    Write-Host ""
    Write-Host "✗ Error executing migration:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Stack trace:" -ForegroundColor Gray
    Write-Host $_.ScriptStackTrace -ForegroundColor Gray
    exit 1
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
