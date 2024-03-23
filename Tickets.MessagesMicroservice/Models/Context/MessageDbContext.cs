using Microsoft.EntityFrameworkCore;
using Tickets.MessagesMicroservice.Models.Entities;

namespace Tickets.MessagesMicroservice.Models.Context
{
    public class MessageDbContext : DbContext
    {
        #region DBSets

        DbSet<Message> MessagesDB;

        #endregion
    }
}
