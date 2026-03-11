using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoM.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddedFewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AbsentCount",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "Attendees",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "PresentCount",
                table: "Meetings");

            migrationBuilder.AddColumn<string>(
                name: "MeetingType",
                table: "Meetings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MeetingUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeetingId = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsPresent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingUsers_Meetings_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "Meetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingUsers_MeetingId",
                table: "MeetingUsers",
                column: "MeetingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeetingUsers");

            migrationBuilder.DropColumn(
                name: "MeetingType",
                table: "Meetings");

            migrationBuilder.AddColumn<int>(
                name: "AbsentCount",
                table: "Meetings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Attendees",
                table: "Meetings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PresentCount",
                table: "Meetings",
                type: "int",
                nullable: true);
        }
    }
}
