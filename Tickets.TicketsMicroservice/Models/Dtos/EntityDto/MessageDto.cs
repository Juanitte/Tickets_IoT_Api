namespace Tickets.MessagesMicroservice.Models.Dtos.EntityDto
{
    public class MessageDto
    {
        public string Content { get; set; }
        public List<IFormFile?> Attachments { get; set; }
        public int TicketId { get; set; }

        public MessageDto()
        {
            this.Content = string.Empty;
            this.Attachments = new List<IFormFile?>();
            this.TicketId = 0;
        }

        public MessageDto(string content, int ticketId)
        {
            this.Content = content;
            this.Attachments = new List<IFormFile?>();
            this.TicketId = ticketId;
        }

        public MessageDto(string content, List<IFormFile?> attachments, int ticketId)
        {
            this.Content = content;
            this.Attachments = attachments;
            this.TicketId = ticketId;
        }
    }
}
