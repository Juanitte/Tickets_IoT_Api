using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickets.UsersMicroservice.Migrations
{
    /// <inheritdoc />
    public partial class Db4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ticketIds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ticketIds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticketIds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ticketIds_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id");
                });
        }
    }
}
