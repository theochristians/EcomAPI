using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EComAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalStockAndFixVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalStock",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalStock",
                table: "Products");
        }
    }
}
