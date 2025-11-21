using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMovie.Migrations
{
    /// <inheritdoc />
    public partial class AddIsHiddenToCustomMovieTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "CustomMovieTitles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "CustomMovieTitles");
        }
    }
}
