using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicShop.Migrations
{
    /// <inheritdoc />
    public partial class AddDualCategorySystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Albums",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ArtistCategoryId",
                table: "Albums",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductTypeId",
                table: "Albums",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ArtistCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Albums_ArtistCategoryId",
                table: "Albums",
                column: "ArtistCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_ProductTypeId",
                table: "Albums",
                column: "ProductTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Albums_ArtistCategories_ArtistCategoryId",
                table: "Albums",
                column: "ArtistCategoryId",
                principalTable: "ArtistCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Albums_ProductTypes_ProductTypeId",
                table: "Albums",
                column: "ProductTypeId",
                principalTable: "ProductTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Albums_ArtistCategories_ArtistCategoryId",
                table: "Albums");

            migrationBuilder.DropForeignKey(
                name: "FK_Albums_ProductTypes_ProductTypeId",
                table: "Albums");

            migrationBuilder.DropTable(
                name: "ArtistCategories");

            migrationBuilder.DropTable(
                name: "ProductTypes");

            migrationBuilder.DropIndex(
                name: "IX_Albums_ArtistCategoryId",
                table: "Albums");

            migrationBuilder.DropIndex(
                name: "IX_Albums_ProductTypeId",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "ArtistCategoryId",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "ProductTypeId",
                table: "Albums");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Albums",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
