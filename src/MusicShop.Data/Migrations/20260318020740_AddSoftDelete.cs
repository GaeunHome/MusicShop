using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicShop.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ProductTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProductTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Banners",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Banners",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Artists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Artists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ArtistCategories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ArtistCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Albums",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Albums",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ProductTypes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProductTypes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ArtistCategories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ArtistCategories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Albums");
        }
    }
}
