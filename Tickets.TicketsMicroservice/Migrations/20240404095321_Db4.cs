using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickets.TicketsMicroservice.Migrations
{
    /// <inheritdoc />
    public partial class Db4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasNewMessages",
                table: "Tickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Timestamp",
                table: "Messages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasNewMessages",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Messages");
        }
    }
}
