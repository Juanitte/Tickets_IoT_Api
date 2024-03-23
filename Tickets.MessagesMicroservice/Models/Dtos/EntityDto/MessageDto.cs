namespace Tickets.MessagesMicroservice.Models.Dtos.EntityDto
{
    public class MessageDto
    {
        public string Content { get; set; }
        public List<IFormFile?> Attachments { get; set; }
        public int TicketID { get; set; }

        public MessageDto()
        {
            this.Content = string.Empty;
            this.Attachments = new List<IFormFile?>();
            this.TicketID = 0;
        }

        public MessageDto(string content, int ticketId)
        {
            this.Content = content;
            this.Attachments = new List<IFormFile?>();
            this.TicketID = ticketId;
        }

        public MessageDto(string content, List<IFormFile?> attachments, int ticketId)
        {
            this.Content = content;
            this.Attachments = attachments;
            this.TicketID = ticketId;
        }
    }
}
