using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KindRaise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Removed_CampaignState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CampaignState",
                table: "Campaigns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CampaignState",
                table: "Campaigns",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
