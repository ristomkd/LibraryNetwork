using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryNetwork.Migrations
{
    /// <inheritdoc />
    public partial class New : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Members",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Members",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "UserId", "UserId1" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "UserId", "UserId1" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "UserId", "UserId1" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "UserId", "UserId1" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Members_UserId1",
                table: "Members",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Members_AspNetUsers_UserId1",
                table: "Members",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_AspNetUsers_UserId1",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_UserId1",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "AspNetUsers");
        }
    }
}
