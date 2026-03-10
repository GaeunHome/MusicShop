using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicShop.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAlbumArtistCategoryId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Albums_ArtistCategories_ArtistCategoryId",
                table: "Albums");

            migrationBuilder.AddForeignKey(
                name: "FK_Albums_ArtistCategories_ArtistCategoryId",
                table: "Albums",
                column: "ArtistCategoryId",
                principalTable: "ArtistCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Albums_ArtistCategories_ArtistCategoryId",
                table: "Albums");

            migrationBuilder.AddForeignKey(
                name: "FK_Albums_ArtistCategories_ArtistCategoryId",
                table: "Albums",
                column: "ArtistCategoryId",
                principalTable: "ArtistCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
