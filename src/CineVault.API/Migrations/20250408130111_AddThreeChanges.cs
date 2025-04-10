using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineVault.API.Migrations
{
    /// <inheritdoc />
    public partial class AddThreeChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Reviews_ReviewId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Users_UserId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_UserId",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Users",
                newName: "Created_At");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Reviews",
                newName: "User_ID");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Reviews",
                newName: "Created_At");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                newName: "IX_Reviews_User_ID");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Likes",
                newName: "User_ID");

            migrationBuilder.RenameColumn(
                name: "ReviewId",
                table: "Likes",
                newName: "Review_ID");

            migrationBuilder.RenameIndex(
                name: "IX_Likes_UserId",
                table: "Likes",
                newName: "IX_Likes_User_ID");

            migrationBuilder.RenameIndex(
                name: "IX_Likes_ReviewId",
                table: "Likes",
                newName: "IX_Likes_Review_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Reviews_Review_ID",
                table: "Likes",
                column: "Review_ID",
                principalTable: "Reviews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Users_User_ID",
                table: "Likes",
                column: "User_ID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_User_ID",
                table: "Reviews",
                column: "User_ID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Reviews_Review_ID",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Users_User_ID",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_User_ID",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "Created_At",
                table: "Users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "User_ID",
                table: "Reviews",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Created_At",
                table: "Reviews",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_User_ID",
                table: "Reviews",
                newName: "IX_Reviews_UserId");

            migrationBuilder.RenameColumn(
                name: "User_ID",
                table: "Likes",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Review_ID",
                table: "Likes",
                newName: "ReviewId");

            migrationBuilder.RenameIndex(
                name: "IX_Likes_User_ID",
                table: "Likes",
                newName: "IX_Likes_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Likes_Review_ID",
                table: "Likes",
                newName: "IX_Likes_ReviewId");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Reviews_ReviewId",
                table: "Likes",
                column: "ReviewId",
                principalTable: "Reviews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Users_UserId",
                table: "Likes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_UserId",
                table: "Reviews",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
