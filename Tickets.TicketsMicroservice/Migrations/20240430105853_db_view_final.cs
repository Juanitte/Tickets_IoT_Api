using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickets.TicketsMicroservice.Migrations
{
    /// <inheritdoc />
    public partial class db_view_final : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE VIEW Tickets_Username AS \r\n\t" +
                "SELECT t.[Id],\r\n\tt.[Title],\r\n\tt.[Name],\r\n\tt.[Email],\r\n\t" +
                "t.[Timestamp],\r\n\tt.[UserId],\r\n\tt.[Priority],\r\n\tt.[IsAsigned],\r\n\t" +
                "t.[HasNewMessages],\r\n\tt.[newMessagesCount],\r\n\tt.[Status],\r\n\tCASE \r\n" +
                "WHEN t.[UserId] = -1 THEN ''\r\n ELSE u.[FullName]\r\n END AS FullName\r\n\t" +
                "FROM [IoT_Tickets_Tickets].[dbo].[Tickets] t\r\n\t" +
                "LEFT JOIN [IoT_Tickets_Users].[dbo].[Users] u ON t.[UserId] = u.[Id] OR t.[UserId] = -1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW Tickets_Username;");
        }
    }
}
