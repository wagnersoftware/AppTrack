using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Identity.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2b7cfb44-36a8-49c9-8a8a-6e9c85a2cf1b",
                columns: new[] { "FirstName", "LastName" },
                values: new object[] { "System", "User" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f8e1c1b9-3f1b-4c24-9b71-17d6a916e42e",
                columns: new[] { "FirstName", "LastName" },
                values: new object[] { "System", "Admin" });
        }
    }
}
