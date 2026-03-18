using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicShop.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUniqueIndexesWithSoftDeleteFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FeaturedArtists_ArtistId",
                table: "FeaturedArtists");

            migrationBuilder.DropIndex(
                name: "IX_Coupons_Code",
                table: "Coupons");

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedArtists_ArtistId",
                table: "FeaturedArtists",
                column: "ArtistId",
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_Code",
                table: "Coupons",
                column: "Code",
                unique: true,
                filter: "IsDeleted = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FeaturedArtists_ArtistId",
                table: "FeaturedArtists");

            migrationBuilder.DropIndex(
                name: "IX_Coupons_Code",
                table: "Coupons");

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedArtists_ArtistId",
                table: "FeaturedArtists",
                column: "ArtistId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_Code",
                table: "Coupons",
                column: "Code",
                unique: true);
        }
    }
}
