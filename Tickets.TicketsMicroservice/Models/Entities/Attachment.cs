using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Tickets.TicketsMicroservice.Models.Entities
{
    [Table("Attachments")]
    public class Attachment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Path { get; set; }
        public int MessageId { get; set; }
        [ForeignKey("MessageId")]
        public Message Message { get; set; }

        public Attachment()
        {
            this.Path = string.Empty;
        }

        public Attachment(string path, int MessageId)
        {
            this.Path = path;
            this.MessageId = MessageId;
        }
    }
}
