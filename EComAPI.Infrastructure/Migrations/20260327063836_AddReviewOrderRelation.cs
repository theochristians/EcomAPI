using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EComAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewOrderRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId_ProductId",
                table: "Reviews");

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "Reviews",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(@"
                ;WITH RankedOrders AS (
                    SELECT
                        r.Id AS ReviewId,
                        o.Id AS OrderId,
                        ROW_NUMBER() OVER (
                            PARTITION BY r.Id
                            ORDER BY
                                CASE WHEN o.Status = 'completed' THEN 0 ELSE 1 END,
                                o.CreatedAt DESC
                        ) AS rn
                    FROM Reviews r
                    INNER JOIN Orders o ON o.UserId = r.UserId AND o.DeletedAt IS NULL
                    INNER JOIN OrderItems oi ON oi.OrderId = o.Id AND oi.DeletedAt IS NULL
                    INNER JOIN ProductVariants pv ON pv.Id = oi.ProductVariantId AND pv.DeletedAt IS NULL
                    WHERE pv.ProductId = r.ProductId
                )
                UPDATE r
                SET r.OrderId = ro.OrderId
                FROM Reviews r
                INNER JOIN RankedOrders ro
                    ON ro.ReviewId = r.Id
                   AND ro.rn = 1;
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM Reviews WHERE OrderId IS NULL)
                    THROW 50001, 'Cannot map existing reviews to orders. Please fix review data before applying migration.', 1;
            ");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrderId",
                table: "Reviews",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_OrderId",
                table: "Reviews",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId_OrderId_ProductId",
                table: "Reviews",
                columns: new[] { "UserId", "OrderId", "ProductId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Orders_OrderId",
                table: "Reviews",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Orders_OrderId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_OrderId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId_OrderId_ProductId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Reviews");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId_ProductId",
                table: "Reviews",
                columns: new[] { "UserId", "ProductId" },
                unique: true);
        }
    }
}
