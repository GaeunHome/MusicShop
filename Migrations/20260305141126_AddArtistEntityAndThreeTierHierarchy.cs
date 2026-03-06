using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicShop.Migrations
{
    /// <inheritdoc />
    public partial class AddArtistEntityAndThreeTierHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Artist",
                table: "Albums");

            migrationBuilder.AddColumn<int>(
                name: "ArtistId",
                table: "Albums",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Artists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ArtistCategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Artists_ArtistCategories_ArtistCategoryId",
                        column: x => x.ArtistCategoryId,
                        principalTable: "ArtistCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Albums_ArtistId",
                table: "Albums",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_ArtistCategoryId",
                table: "Artists",
                column: "ArtistCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Albums_Artists_ArtistId",
                table: "Albums",
                column: "ArtistId",
                principalTable: "Artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Albums_Artists_ArtistId",
                table: "Albums");

            migrationBuilder.DropTable(
                name: "Artists");

            migrationBuilder.DropIndex(
                name: "IX_Albums_ArtistId",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "ArtistId",
                table: "Albums");

            migrationBuilder.AddColumn<string>(
                name: "Artist",
                table: "Albums",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
