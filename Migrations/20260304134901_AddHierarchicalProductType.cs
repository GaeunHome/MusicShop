using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicShop.Migrations
{
    /// <inheritdoc />
    public partial class AddHierarchicalProductType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "ProductTypes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductTypes_ParentId",
                table: "ProductTypes",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTypes_ProductTypes_ParentId",
                table: "ProductTypes",
                column: "ParentId",
                principalTable: "ProductTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductTypes_ProductTypes_ParentId",
                table: "ProductTypes");

            migrationBuilder.DropIndex(
                name: "IX_ProductTypes_ParentId",
                table: "ProductTypes");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "ProductTypes");
        }
    }
}
