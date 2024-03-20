using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Tickets.MessagesMicroservice.Models.Entities
{
    [Table("Messages")]
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Content { get; set; }
        public List<Attachment?> AttachmentPaths { get; set; } = new List<Attachment?>();
        public int TicketID { get; set; }

        public Message()
        {
            Id = 0;
            Content = string.Empty;
            TicketID = 0;
        }

        public Message(string content, int ticketId)
        {
            Id = 0;
            Content = content;
            TicketID = ticketId;
        }

        public Message(string content, List<Attachment?> attachmentPaths, int ticketId)
        {
            Id = 0;
            Content = content;
            AttachmentPaths = attachmentPaths;
            TicketID = ticketId;
        }
    }
}
