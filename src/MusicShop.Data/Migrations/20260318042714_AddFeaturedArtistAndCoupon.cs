using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicShop.Migrations
{
    /// <inheritdoc />
    public partial class AddFeaturedArtistAndCoupon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Albums_ArtistCategories_ArtistCategoryId",
                table: "Albums");

            migrationBuilder.DropIndex(
                name: "IX_Albums_ArtistCategoryId",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "ArtistCategoryId",
                table: "Albums");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Orders",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "UserCouponId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DiscountType = table.Column<int>(type: "int", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MaxDiscountAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ValidDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsRedeemable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeaturedArtists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArtistId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeaturedArtists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeaturedArtists_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCoupons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    Source = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCoupons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCoupons_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCoupons_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserCoupons_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserCouponId",
                table: "Orders",
                column: "UserCouponId");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_Code",
                table: "Coupons",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedArtists_ArtistId",
                table: "FeaturedArtists",
                column: "ArtistId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCoupons_CouponId",
                table: "UserCoupons",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCoupons_OrderId",
                table: "UserCoupons",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCoupons_UserId",
                table: "UserCoupons",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_UserCoupons_UserCouponId",
                table: "Orders",
                column: "UserCouponId",
                principalTable: "UserCoupons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_UserCoupons_UserCouponId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "FeaturedArtists");

            migrationBuilder.DropTable(
                name: "UserCoupons");

            migrationBuilder.DropTable(
                name: "Coupons");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UserCouponId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UserCouponId",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "ArtistCategoryId",
                table: "Albums",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Albums_ArtistCategoryId",
                table: "Albums",
                column: "ArtistCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Albums_ArtistCategories_ArtistCategoryId",
                table: "Albums",
                column: "ArtistCategoryId",
                principalTable: "ArtistCategories",
                principalColumn: "Id");
        }
    }
}
