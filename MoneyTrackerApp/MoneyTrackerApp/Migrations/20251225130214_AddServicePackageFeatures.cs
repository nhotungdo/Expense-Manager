using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyTrackerApp.Migrations
{
    /// <inheritdoc />
    public partial class AddServicePackageFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Columns already exist in DB (created via SQL script), so we don't need to add them.
            // This migration just syncs the EF model snapshot.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingCycle",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "HasAdvancedReports",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "HasAiAdvisor",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "HasGroupExpense",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "HasPrioritySupport",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "MaxAccounts",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "MaxBudgets",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "MaxTransactions",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "PackageType",
                table: "ServicePackages");
        }
    }
}
