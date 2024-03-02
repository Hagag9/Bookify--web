using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bookify.Web.Data.Migrations
{
    public partial class modifyNameOfIsAvailableCol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsAvilableForRental",
                table: "Books",
                newName: "IsAvailableForRental");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsAvailableForRental",
                table: "Books",
                newName: "IsAvilableForRental");
        }
    }
}
