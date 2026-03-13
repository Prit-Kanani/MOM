using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoM.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateForLogins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByAuthUserId",
                table: "Meetings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuthUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordSalt = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthUsers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_CreatedByAuthUserId",
                table: "Meetings",
                column: "CreatedByAuthUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthUsers_UserName",
                table: "AuthUsers",
                column: "UserName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_AuthUsers_CreatedByAuthUserId",
                table: "Meetings",
                column: "CreatedByAuthUserId",
                principalTable: "AuthUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_AuthUsers_CreatedByAuthUserId",
                table: "Meetings");

            migrationBuilder.DropTable(
                name: "AuthUsers");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_CreatedByAuthUserId",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "CreatedByAuthUserId",
                table: "Meetings");
        }
    }
}
