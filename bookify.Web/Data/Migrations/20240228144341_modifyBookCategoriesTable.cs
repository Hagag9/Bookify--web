using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bookify.Web.Data.Migrations
{
    public partial class modifyBookCategoriesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookCtegories_Books_BookId",
                table: "BookCtegories");

            migrationBuilder.DropForeignKey(
                name: "FK_BookCtegories_Categories_CategoryId",
                table: "BookCtegories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookCtegories",
                table: "BookCtegories");

            migrationBuilder.RenameTable(
                name: "BookCtegories",
                newName: "BookCategories");

            migrationBuilder.RenameIndex(
                name: "IX_BookCtegories_CategoryId",
                table: "BookCategories",
                newName: "IX_BookCategories_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookCategories",
                table: "BookCategories",
                columns: new[] { "BookId", "CategoryId" });

            migrationBuilder.AddForeignKey(
                name: "FK_BookCategories_Books_BookId",
                table: "BookCategories",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookCategories_Categories_CategoryId",
                table: "BookCategories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookCategories_Books_BookId",
                table: "BookCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_BookCategories_Categories_CategoryId",
                table: "BookCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookCategories",
                table: "BookCategories");

            migrationBuilder.RenameTable(
                name: "BookCategories",
                newName: "BookCtegories");

            migrationBuilder.RenameIndex(
                name: "IX_BookCategories_CategoryId",
                table: "BookCtegories",
                newName: "IX_BookCtegories_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookCtegories",
                table: "BookCtegories",
                columns: new[] { "BookId", "CategoryId" });

            migrationBuilder.AddForeignKey(
                name: "FK_BookCtegories_Books_BookId",
                table: "BookCtegories",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookCtegories_Categories_CategoryId",
                table: "BookCtegories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
