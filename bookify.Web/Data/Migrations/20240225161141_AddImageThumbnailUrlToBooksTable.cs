using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bookify.Web.Data.Migrations
{
    public partial class AddImageThumbnailUrlToBooksTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageThumbnailUrl",
                table: "Books",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageThumbnailUrl",
                table: "Books");
        }
    }
}
