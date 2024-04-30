using Microsoft.EntityFrameworkCore;
using Tickets.TicketsMicroservice.Models.Dtos.EntityDto;
using Tickets.TicketsMicroservice.Models.Entities;

namespace Tickets.TicketsMicroservice.Models.Context
{
    public class TicketsDbContext : DbContext
    {
        #region DBSets

        public DbSet<Ticket> TicketDb { get; set; }
        public DbSet<Message> MessagesDb { get; set; }
        public DbSet<TicketUser> TicketUserDb { get; set; }

        #endregion

        public TicketsDbContext(DbContextOptions options)
        : base(options)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="modelBuilder"><see cref="ModelBuilder"/></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Deshabilita la eliminación en cascada
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ticket>().ToTable("Tickets");
            modelBuilder.Entity<Message>().ToTable("Messages");
            modelBuilder.Entity<Attachment>().ToTable("Attachments");
            modelBuilder.Entity<TicketUser>().ToView("Tickets_Username").HasKey(t => t.Id);

            modelBuilder.Entity<Ticket>()
                .HasMany(t => t.Messages)
                .WithOne(m => m.Ticket)
                .HasForeignKey(m => m.TicketId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Attachment>()
                .HasOne(t => t.Message)
                .WithMany(m => m.AttachmentPaths)
                .HasForeignKey(t => t.MessageId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
