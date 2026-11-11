using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KindRaise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Campaigns",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "Campaigns");
        }
    }
}
