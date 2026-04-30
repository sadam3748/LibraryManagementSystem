using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddBorrowTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReturned",
                table: "BorrowTransactions");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "BorrowTransactions",
                newName: "Status");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "BorrowTransactions",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "FineAmount",
                table: "BorrowTransactions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnDate",
                table: "BorrowTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BorrowTransactions_ApplicationUserId",
                table: "BorrowTransactions",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowTransactions_BookId",
                table: "BorrowTransactions",
                column: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowTransactions_AspNetUsers_ApplicationUserId",
                table: "BorrowTransactions",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowTransactions_Books_BookId",
                table: "BorrowTransactions",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowTransactions_AspNetUsers_ApplicationUserId",
                table: "BorrowTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_BorrowTransactions_Books_BookId",
                table: "BorrowTransactions");

            migrationBuilder.DropIndex(
                name: "IX_BorrowTransactions_ApplicationUserId",
                table: "BorrowTransactions");

            migrationBuilder.DropIndex(
                name: "IX_BorrowTransactions_BookId",
                table: "BorrowTransactions");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "BorrowTransactions");

            migrationBuilder.DropColumn(
                name: "FineAmount",
                table: "BorrowTransactions");

            migrationBuilder.DropColumn(
                name: "ReturnDate",
                table: "BorrowTransactions");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "BorrowTransactions",
                newName: "UserId");

            migrationBuilder.AddColumn<bool>(
                name: "IsReturned",
                table: "BorrowTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
