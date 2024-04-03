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
        public List<Attachment?> AttachmentPaths { get; set; } = new List<Attachment?>();
        public int TicketId { get; set; }
        [ForeignKey("TicketId")]
        public Ticket? Ticket { get; set; }

        public Message()
        {
            Id = 0;
            Author = string.Empty;
            Content = string.Empty;
            TicketId = 0;
            Ticket = null;
        }

        public Message(string content, string author, int ticketId)
        {
            Id = 0;
            Author = author;
            Content = content;
            TicketId = ticketId;
            Ticket = null;
        }

        public Message(string content, string author, List<Attachment?> attachmentPaths, int ticketId)
        {
            Id = 0;
            Author = author;
            Content = content;
            AttachmentPaths = attachmentPaths;
            TicketId = ticketId;
            Ticket = null;
        }
    }
}
