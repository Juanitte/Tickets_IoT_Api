using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickets.TicketsMicroservice.Migrations
{
    /// <inheritdoc />
    public partial class Db5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "newMessagesCount",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "newMessagesCount",
                table: "Tickets");
        }
    }
}
