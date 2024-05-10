using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Tickets.TicketsMicroservice.Models.Entities
{
    [Table("Messages")]
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public List<Attachment?> AttachmentPaths { get; set; } = new List<Attachment?>();
        public bool IsTechnician { get; set; }
        public int TicketId { get; set; }
        [ForeignKey("TicketId")]
        public Ticket? Ticket { get; set; }

        public Message()
        {
            Id = 0;
            Author = string.Empty;
            Content = string.Empty;
            this.Timestamp = DateTime.Now;
            TicketId = 0;
            Ticket = null;
            IsTechnician = false;
        }

        public Message(string content, string author, int ticketId)
        {
            Id = 0;
            Author = author;
            Content = content;
            this.Timestamp = DateTime.Now;
            TicketId = ticketId;
            Ticket = null;
            IsTechnician = false;
        }

        public Message(string content, string author, int ticketId, bool isTechnician)
        {
            Id= 0;
            Author = author;
            Content = content;
            this.Timestamp = DateTime.Now;
            TicketId = ticketId;
            Ticket = null;
            IsTechnician = isTechnician;
        }

        public Message(string content, string author, List<Attachment?> attachmentPaths, int ticketId, bool isTechnician)
        {
            Id = 0;
            Author = author;
            Content = content;
            this.Timestamp = DateTime.Now;
            AttachmentPaths = attachmentPaths;
            TicketId = ticketId;
            Ticket = null;
            IsTechnician = isTechnician;
        }
    }
}
