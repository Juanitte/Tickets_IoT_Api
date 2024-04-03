namespace Tickets.MessagesMicroservice.Models.Dtos.EntityDto
{
    public class MessageDto
    {
        public string Author { get; set; }
        public string Content { get; set; }
        public List<IFormFile?> Attachments { get; set; }
        public int TicketId { get; set; }

        public MessageDto()
        {
            this.Author = string.Empty;
            this.Content = string.Empty;
            this.Attachments = new List<IFormFile?>();
            this.TicketId = 0;
        }

        public MessageDto(string author, string content, int ticketId)
        {
            this.Author = author;
            this.Content = content;
            this.Attachments = new List<IFormFile?>();
            this.TicketId = ticketId;
        }

        public MessageDto(string author, string content, List<IFormFile?> attachments, int ticketId)
        {
            this.Author = author;
            this.Content = content;
            this.Attachments = attachments;
            this.TicketId = ticketId;
        }
    }
}
