using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoM.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerUserMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Owner",
                table: "AgendaItems");

            migrationBuilder.DropColumn(
                name: "Responsibility",
                table: "ActionItems");

            migrationBuilder.AddColumn<int>(
                name: "OwnerUserId",
                table: "AgendaItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResponsibilityUserId",
                table: "ActionItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgendaItems_OwnerUserId",
                table: "AgendaItems",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionItems_ResponsibilityUserId",
                table: "ActionItems",
                column: "ResponsibilityUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionItems_Users_ResponsibilityUserId",
                table: "ActionItems",
                column: "ResponsibilityUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AgendaItems_Users_OwnerUserId",
                table: "AgendaItems",
                column: "OwnerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionItems_Users_ResponsibilityUserId",
                table: "ActionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_AgendaItems_Users_OwnerUserId",
                table: "AgendaItems");

            migrationBuilder.DropIndex(
                name: "IX_AgendaItems_OwnerUserId",
                table: "AgendaItems");

            migrationBuilder.DropIndex(
                name: "IX_ActionItems_ResponsibilityUserId",
                table: "ActionItems");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "AgendaItems");

            migrationBuilder.DropColumn(
                name: "ResponsibilityUserId",
                table: "ActionItems");

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "AgendaItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Responsibility",
                table: "ActionItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
