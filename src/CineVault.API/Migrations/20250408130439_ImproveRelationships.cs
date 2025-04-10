using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineVault.API.Migrations
{
    /// <inheritdoc />
    public partial class ImproveRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_User_ID",
                table: "Reviews");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_User_ID",
                table: "Reviews",
                column: "User_ID",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_User_ID",
                table: "Reviews");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_User_ID",
                table: "Reviews",
                column: "User_ID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
