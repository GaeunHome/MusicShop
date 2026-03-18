using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicShop.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFeaturedArtistUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FeaturedArtists_ArtistId",
                table: "FeaturedArtists");

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedArtists_ArtistId",
                table: "FeaturedArtists",
                column: "ArtistId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FeaturedArtists_ArtistId",
                table: "FeaturedArtists");

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedArtists_ArtistId",
                table: "FeaturedArtists",
                column: "ArtistId",
                unique: true,
                filter: "IsDeleted = 0");
        }
    }
}
