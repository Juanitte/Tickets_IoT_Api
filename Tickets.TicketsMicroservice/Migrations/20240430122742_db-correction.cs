using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickets.TicketsMicroservice.Migrations
{
    /// <inheritdoc />
    public partial class dbcorrection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsAsigned",
                table: "Tickets",
                newName: "IsAssigned");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsAssigned",
                table: "Tickets",
                newName: "IsAsigned");
        }
    }
}
