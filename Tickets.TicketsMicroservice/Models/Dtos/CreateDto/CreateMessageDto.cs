namespace Tickets.TicketsMicroservice.Models.Dtos.CreateDto
{
    public class CreateMessageDto
    {
        public string Author { get; set; }
        public string Content { get; set; }
        public List<IFormFile?> Attachments { get; set; }
        public int TicketId { get; set; }

        public CreateMessageDto()
        {
            Author = string.Empty;
            Content = string.Empty;
            Attachments = new List<IFormFile?>();
            TicketId = 0;
        }

        public CreateMessageDto(string author, string content, int ticketId)
        {
            Author = author;
            Content = content;
            Attachments = new List<IFormFile?>();
            TicketId = ticketId;
        }

        public CreateMessageDto(string author, string content, List<IFormFile?> attachments, int ticketId)
        {
            Author = author;
            Content = content;
            Attachments = attachments;
            TicketId = ticketId;
        }
    }
}
