using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeatFinder.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeatPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeatPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    PreferredRowFrom = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PreferredRowTo = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CenterSeatFrom = table.Column<int>(type: "integer", nullable: false),
                    CenterSeatTo = table.Column<int>(type: "integer", nullable: false),
                    AvoidAisles = table.Column<bool>(type: "boolean", nullable: false),
                    PartySize = table.Column<int>(type: "integer", nullable: false),
                    SearchRadiusMiles = table.Column<int>(type: "integer", nullable: false),
                    HomeTheaterId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatPreferences", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeatPreferences");
        }
    }
}
