using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CharliesSnackBar.Data.Migrations
{
    public partial class UpdatePreviousMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItem_Category_Categoryd",
                table: "MenuItem");

            migrationBuilder.DropIndex(
                name: "IX_MenuItem_Categoryd",
                table: "MenuItem");

            migrationBuilder.DropColumn(
                name: "Categoryd",
                table: "MenuItem");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItem_CategoryId",
                table: "MenuItem",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItem_Category_CategoryId",
                table: "MenuItem",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItem_Category_CategoryId",
                table: "MenuItem");

            migrationBuilder.DropIndex(
                name: "IX_MenuItem_CategoryId",
                table: "MenuItem");

            migrationBuilder.AddColumn<int>(
                name: "Categoryd",
                table: "MenuItem",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItem_Categoryd",
                table: "MenuItem",
                column: "Categoryd");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItem_Category_Categoryd",
                table: "MenuItem",
                column: "Categoryd",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
